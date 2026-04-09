# STAGE 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /app

# Cài đặt các công cụ cần thiết cho quá trình build
RUN apk add --no-cache icu-libs

# Copy file giải pháp và csproj để restore (tận dụng lớp layer cache)
COPY *.sln .
COPY HeartLink/*.csproj ./HeartLink/
RUN dotnet restore "./HeartLink/HeartLink.csproj"

# Copy toàn bộ mã nguồn
COPY . .
WORKDIR "/app/HeartLink"

# Build ứng dụng với tùy chọn tối ưu hóa dung lượng
# Trimmed (nếu bạn muốn app siêu nhỏ, nhưng cần test kỹ) - Ở đây dùng Publish tiêu chuẩn cho ổn định
RUN dotnet publish "HeartLink.csproj" -c Release -o /app/publish /p:UseAppHost=false

# STAGE 2: Runtime (Bản rút gọn siêu nhẹ)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

# Cài đặt ICU (tiếng Việt) và ca-certificates (bắt buộc để kết nối Aiven MySQL qua SSL)
RUN apk add --no-cache icu-libs ca-certificates
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Cấu hình biến môi trường để chống tràn RAM trên Render
ENV \
    # 1. Chuyển sang Workstation GC (Tiết kiệm RAM hơn Server GC)
    DOTNET_gcServer=0 \
    # 2. Ép dọn dẹp RAM khi đạt ngưỡng (Render Free thường có 512MB RAM)
    DOTNET_GCHeapHardLimitPercent=70 \
    # 3. Chạy trên cổng 8080 cho Render
    ASPNETCORE_URLS=http://+:8080 \
    # 4. Chế độ Production
    ASPNETCORE_ENVIRONMENT=Production

# Copy file đã build từ Stage 1
COPY --from=build /app/publish .

# Bảo mật: Chạy với quyền User hạn chế (không dùng root)
USER $APP_UID

# Lệnh khởi chạy
ENTRYPOINT ["dotnet", "HeartLink.dll"]