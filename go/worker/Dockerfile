FROM golang:1.24-alpine AS build
WORKDIR /src
COPY go.mod go.sum ./
RUN go mod download
COPY . .
RUN CGO_ENABLED=0 go build -o /app/worker ./cmd/worker/

FROM alpine:3.21
WORKDIR /app
COPY --from=build /app/worker .
ENTRYPOINT ["./worker"]
