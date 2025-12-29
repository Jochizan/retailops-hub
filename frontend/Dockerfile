# Stage 1: Build Angular
FROM node:20 AS build
WORKDIR /app
COPY frontend/package*.json ./
RUN npm ci
COPY frontend/ .
RUN npm run build -- --configuration production

# Stage 2: Serve with Nginx
FROM nginx:alpine
COPY --from=build /app/dist/retailops-ui/browser /usr/share/nginx/html
# Optional: Copy custom nginx config if needed for SPA routing
COPY frontend/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
