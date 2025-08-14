#!/bin/bash
set -e

# Create multiple databases for microservices architecture
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    -- Create additional databases for microservices
    CREATE DATABASE auth_dev;
    CREATE DATABASE notifications_dev;
    CREATE DATABASE posts_dev;
    CREATE DATABASE users_dev;
    CREATE DATABASE likes_dev;
    CREATE DATABASE comments_dev;
    CREATE DATABASE media_dev;

    -- Grant privileges to webapp_user for all databases
    GRANT ALL PRIVILEGES ON DATABASE webapp_dev TO webapp_user;
    GRANT ALL PRIVILEGES ON DATABASE auth_dev TO webapp_user;
    GRANT ALL PRIVILEGES ON DATABASE notifications_dev TO webapp_user;
    GRANT ALL PRIVILEGES ON DATABASE posts_dev TO webapp_user;
    GRANT ALL PRIVILEGES ON DATABASE users_dev TO webapp_user;
    GRANT ALL PRIVILEGES ON DATABASE likes_dev TO webapp_user;
    GRANT ALL PRIVILEGES ON DATABASE comments_dev TO webapp_user;
    GRANT ALL PRIVILEGES ON DATABASE media_dev TO webapp_user;
EOSQL

echo "Multiple databases created successfully for microservices development!"
