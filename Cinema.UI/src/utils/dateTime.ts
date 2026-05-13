import dayjs from './dayjs';

const EXPLICIT_TIME_ZONE_PATTERN = /(Z|[+-]\d{2}:?\d{2})$/i;

export const toLocalDateTime = (value: string | Date) => {
  if (value instanceof Date) {
    return dayjs.utc(value).local();
  }

  const normalizedValue = EXPLICIT_TIME_ZONE_PATTERN.test(value)
    ? value
    : `${value}Z`;

  return dayjs.utc(normalizedValue).local();
};
