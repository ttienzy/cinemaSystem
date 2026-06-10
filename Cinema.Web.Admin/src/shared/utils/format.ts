import dayjs from 'dayjs';

export function formatMoney(value?: number | string | null): string {
  return `${Number(value ?? 0).toLocaleString('vi-VN')} VND`;
}

export function formatDateTime(value?: string | null): string {
  return value ? dayjs(value).format('DD/MM/YYYY HH:mm') : '-';
}

export function formatDate(value?: string | null): string {
  return value ? dayjs(value).format('DD/MM/YYYY') : '-';
}

export function getUtcOffsetMinutes(): number {
  return -new Date().getTimezoneOffset();
}
