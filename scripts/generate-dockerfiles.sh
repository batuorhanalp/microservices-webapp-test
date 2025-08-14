#!/bin/bash

# Array of service names and their corresponding project names
declare -a services=(
    "user-service:WebApp.UserService"
    "post-service:WebApp.PostService"
    "notification-service:WebApp.NotificationService"
    "like-service:WebApp.LikeService"
    "comment-service:WebApp.CommentService"
    "media-upload-service:WebApp.MediaUploadService"
    "media-processing-service:WebApp.MediaProcessingService"
)

# Generate Dockerfiles for each service
for service_info in "${services[@]}"; do
    IFS=':' read -r service_dir project_name <<< "$service_info"
    
    # Create the directory path
    dockerfile_path="src/backend/services/${service_dir}/${project_name}/Dockerfile.dev"
    
    echo "Creating Dockerfile for ${project_name}..."
    
    # Create Dockerfile content
    cat > "$dockerfile_path" << EOF
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY src/backend/WebApp.sln .

# Copy project files
COPY src/backend/shared/common/WebApp.Common/WebApp.Common.csproj shared/common/WebApp.Common/
COPY src/backend/services/${service_dir}/${project_name}/${project_name}.csproj services/${service_dir}/${project_name}/

# Restore dependencies
RUN dotnet restore services/${service_dir}/${project_name}/${project_name}.csproj

# Copy all source files
COPY src/backend/shared/common/WebApp.Common/ shared/common/WebApp.Common/
COPY src/backend/services/${service_dir}/${project_name}/ services/${service_dir}/${project_name}/

# Build the project
WORKDIR /src/services/${service_dir}/${project_name}
RUN dotnet build ${project_name}.csproj -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish ${project_name}.csproj -c Debug -o /app/publish

FROM base AS final
WORKDIR /app

# Install debugging tools
USER root
RUN apt-get update && apt-get install -y \\
    curl \\
    procps \\
    && rm -rf /var/lib/apt/lists/*
USER app

COPY --from=publish /app/publish .

# Environment for debugging
ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "${project_name}.dll"]
EOF

    echo "Created ${dockerfile_path}"
done

echo "All Dockerfiles created successfully!"
