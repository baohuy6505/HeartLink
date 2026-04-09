# STAGE 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /app

# Cài đặt công cụ build
RUN apk add --no-cache icu-libs

# Vì Dockerfile nằm cùng cấp với file .csproj, ta copy trực tiếp
# Lưu ý: Nếu có file .sln ở thư mục cha, bạn nên đưa Dockerfile ra ngoài. 
# Nhưng nếu build độc lập project này, hãy dùng:
COPY *.csproj ./
RUN dotnet restore

# Copy toàn bộ code trong thư mục hiện tại vào Docker
COPY . .

# Publish trực tiếp vì đang ở thư mục app
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# STAGE 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app

# Cài đặt thư viện hỗ trợ SSL cho Aiven và Tiếng Việt
RUN apk add --no-cache icu-libs ca-certificates
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Tối ưu RAM cho Render
ENV \
    DOTNET_gcServer=0 \
    DOTNET_GCHeapHardLimitPercent=70 \
    ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

USER $APP_UID

# Chú ý: Kiểm tra file HeartLink.dll có đúng tên project không
ENTRYPOINT ["dotnet", "HeartLink.dll"]