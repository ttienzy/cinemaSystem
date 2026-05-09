const DEFAULT_GATEWAY_URL = 'http://localhost:5200';

export function getApiGatewayBaseUrl(): string {
  return (import.meta.env.VITE_API_GATEWAY_URL || DEFAULT_GATEWAY_URL).replace(/\/+$/, '');
}
