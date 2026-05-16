const DEFAULT_GATEWAY_URL = 'http://localhost:5200';
const DEFAULT_PAYMENT_API_URL = 'https://localhost:7252';

export function getApiGatewayBaseUrl(): string {
  return (import.meta.env.VITE_API_GATEWAY_URL || DEFAULT_GATEWAY_URL).replace(/\/+$/, '');
}

export function getPaymentApiBaseUrl(): string {
  return (import.meta.env.VITE_PAYMENT_API_URL || DEFAULT_PAYMENT_API_URL).replace(/\/+$/, '');
}
