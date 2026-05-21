FROM node:22-alpine AS build

WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .

ARG VITE_API_GATEWAY_URL=http://localhost:5200
ARG VITE_PAYMENT_API_URL=http://localhost:5200
ENV VITE_API_GATEWAY_URL=$VITE_API_GATEWAY_URL
ENV VITE_PAYMENT_API_URL=$VITE_PAYMENT_API_URL

RUN npm run build

FROM nginx:1.27-alpine AS runtime

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
