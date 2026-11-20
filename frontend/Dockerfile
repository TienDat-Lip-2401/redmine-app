# ---- Build ----
FROM node:22-alpine AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
# Vite bakes the API URL into the bundle at build time.
ARG VITE_API_URL=http://localhost:5164/api
ENV VITE_API_URL=$VITE_API_URL
RUN npm run build

# ---- Serve ----
FROM nginx:1.27-alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
