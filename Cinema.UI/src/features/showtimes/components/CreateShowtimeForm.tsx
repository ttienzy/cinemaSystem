import React, { useState } from 'react';
import { InfoCircleOutlined } from '@ant-design/icons';
import { useMutation, useQuery } from '@tanstack/react-query';
import { Alert, Button, Card, DatePicker, Form, InputNumber, Select, Space, Typography, message } from 'antd';
import { cinemaApi } from '../../cinemas/api/cinemaApi';
import { movieApi } from '../../movies/api/movieApi';
import { showtimeApi, type CreateShowtimeRequest } from '../api/showtimeApi';
import dayjs from '../../../utils/dayjs';

const { Option } = Select;
const { Text } = Typography;
const MIN_SHOWTIME_HOUR = 8;

const isFutureShowtimeDate = (value: dayjs.Dayjs) => {
  return value.startOf('day').isAfter(dayjs().startOf('day'));
};

const isValidShowtimeStart = (value: dayjs.Dayjs) => {
  const minimumAllowedTime = value.startOf('day').hour(MIN_SHOWTIME_HOUR).minute(0).second(0).millisecond(0);
  return isFutureShowtimeDate(value) && value.isAfter(minimumAllowedTime);
};

export const CreateShowtimeForm: React.FC = () => {
  const [form] = Form.useForm();
  const [selectedCinemaId, setSelectedCinemaId] = useState<string | null>(null);

  const { data: moviesRes, isLoading: isLoadingMovies } = useQuery({
    queryKey: ['movies-list'],
    queryFn: () => movieApi.getMovies(1, 100),
  });

  const { data: cinemasRes, isLoading: isLoadingCinemas } = useQuery({
    queryKey: ['cinemas-list'],
    queryFn: () => cinemaApi.getCinemas(1, 100),
  });

  const { data: hallsRes, isLoading: isLoadingHalls } = useQuery({
    queryKey: ['halls-list', selectedCinemaId],
    queryFn: () => cinemaApi.getHallsByCinemaId(selectedCinemaId!),
    enabled: !!selectedCinemaId,
  });

  const createMutation = useMutation({
    mutationFn: showtimeApi.createShowtime,
    onSuccess: (res) => {
      if (res.success) {
        message.success('Tao lich chieu thanh cong!');
        form.resetFields();
      } else {
        message.error(res.message || 'Loi khi tao lich chieu');
      }
    },
    onError: () => message.error('Loi ket noi den may chu'),
  });

  const onFinish = (values: {
    movieId: string;
    cinemaHallId: string;
    startTime: dayjs.Dayjs;
    price: number;
  }) => {
    const utcStartTime = dayjs(values.startTime).utc().format();

    const payload: CreateShowtimeRequest = {
      movieId: values.movieId,
      cinemaHallId: values.cinemaHallId,
      startTime: utcStartTime,
      price: values.price,
    };

    createMutation.mutate(payload);
  };

  return (
    <Card title="Tao Lich Chieu Moi" style={{ maxWidth: 600 }}>
      <Form form={form} layout="vertical" onFinish={onFinish}>
        <Alert
          showIcon
          type="info"
          icon={<InfoCircleOutlined />}
          message="Quy dinh gio chieu"
          description="Ngay chieu phai lon hon hom nay va gio chieu phai lon hon 08:00 sang. Moc 08:00 dung gio se khong hop le."
          style={{ marginBottom: 16, borderRadius: 10 }}
        />

        <Form.Item
          name="movieId"
          label="Chon Phim"
          rules={[{ required: true, message: 'Vui long chon phim' }]}
        >
          <Select placeholder="Chon phim" loading={isLoadingMovies} showSearch optionFilterProp="children">
            {moviesRes?.data?.items?.map((movie) => (
              <Option key={movie.id} value={movie.id}>
                {movie.title}
              </Option>
            ))}
          </Select>
        </Form.Item>

        <Form.Item
          name="cinemaId"
          label="Chon Rap"
          rules={[{ required: true, message: 'Vui long chon rap' }]}
        >
          <Select
            placeholder="Chon cum rap"
            loading={isLoadingCinemas}
            onChange={(value) => {
              setSelectedCinemaId(value);
              form.setFieldsValue({ cinemaHallId: undefined });
            }}
          >
            {cinemasRes?.data?.items?.map((cinema) => (
              <Option key={cinema.id} value={cinema.id}>
                {cinema.name}
              </Option>
            ))}
          </Select>
        </Form.Item>

        <Form.Item
          name="cinemaHallId"
          label="Chon Phong Chieu"
          rules={[{ required: true, message: 'Vui long chon phong' }]}
        >
          <Select placeholder="Chon phong chieu" loading={isLoadingHalls} disabled={!selectedCinemaId}>
            {hallsRes?.data?.map((hall) => (
              <Option key={hall.id} value={hall.id}>
                {hall.name} ({hall.totalSeats} ghe)
              </Option>
            ))}
          </Select>
        </Form.Item>

        <Form.Item
          name="startTime"
          label="Ngay Gio Chieu"
          extra={
            <Space size={6}>
              <InfoCircleOutlined style={{ color: '#1677ff' }} />
              <Text type="secondary">Chi cho phep chon ngay sau hom nay va gio sau 08:00 sang.</Text>
            </Space>
          }
          rules={[
            { required: true, message: 'Vui long chon ngay gio' },
            {
              validator: (_, value: dayjs.Dayjs | undefined) => {
                if (!value) {
                  return Promise.resolve();
                }

                if (!isFutureShowtimeDate(value)) {
                  return Promise.reject(new Error('Ngay chieu phai lon hon ngay hom nay'));
                }

                if (!isValidShowtimeStart(value)) {
                  return Promise.reject(new Error('Ngay gio chieu phai lon hon 08:00 sang'));
                }

                return Promise.resolve();
              },
            },
          ]}
        >
          <DatePicker
            disabledDate={(current) => !!current && !current.startOf('day').isAfter(dayjs().startOf('day'))}
            showTime={{
              format: 'HH:mm',
              minuteStep: 5,
              disabledTime: (current) => {
                if (!current) {
                  return {};
                }

                return {
                  disabledHours: () => Array.from({ length: MIN_SHOWTIME_HOUR }, (_, index) => index),
                  disabledMinutes: (selectedHour) => (selectedHour === MIN_SHOWTIME_HOUR ? [0] : []),
                };
              },
            }}
            format="DD/MM/YYYY HH:mm"
            style={{ width: '100%' }}
            placeholder="Chon ngay va gio"
          />
        </Form.Item>

        <Form.Item
          name="price"
          label="Gia ve co ban (VND)"
          rules={[{ required: true, message: 'Vui long nhap gia ve' }]}
        >
          <InputNumber
            style={{ width: '100%' }}
            min={1000}
            step={1000}
            formatter={(value) => `${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
          />
        </Form.Item>

        <Button type="primary" htmlType="submit" loading={createMutation.isPending} block>
          Tao Lich Chieu
        </Button>
      </Form>
    </Card>
  );
};
