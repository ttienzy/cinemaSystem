import React, { useState } from 'react';
import { Form, Select, DatePicker, InputNumber, Button, message, Card } from 'antd';
import { useQuery, useMutation } from '@tanstack/react-query';
import { movieApi } from '../../movies/api/movieApi';
import { cinemaApi } from '../../cinemas/api/cinemaApi';
import { showtimeApi, type CreateShowtimeRequest } from '../api/showtimeApi';
import dayjs from '../../../utils/dayjs';

const { Option } = Select;

export const CreateShowtimeForm: React.FC = () => {
  const [form] = Form.useForm();
  const [selectedCinemaId, setSelectedCinemaId] = useState<string | null>(null);

  // Fetch Movies
  const { data: moviesRes, isLoading: isLoadingMovies } = useQuery({
    queryKey: ['movies-list'],
    queryFn: () => movieApi.getMovies(1, 100),
  });

  // Fetch Cinemas
  const { data: cinemasRes, isLoading: isLoadingCinemas } = useQuery({
    queryKey: ['cinemas-list'],
    queryFn: () => cinemaApi.getCinemas(1, 100),
  });

  // Fetch Halls when Cinema is selected
  const { data: hallsRes, isLoading: isLoadingHalls } = useQuery({
    queryKey: ['halls-list', selectedCinemaId],
    queryFn: () => cinemaApi.getHallsByCinemaId(selectedCinemaId!),
    enabled: !!selectedCinemaId,
  });

  const createMutation = useMutation({
    mutationFn: showtimeApi.createShowtime,
    onSuccess: (res) => {
      if (res.success) {
        message.success('Tạo lịch chiếu thành công!');
        form.resetFields();
      } else {
        message.error(res.message || 'Lỗi khi tạo lịch chiếu');
      }
    },
    onError: () => message.error('Lỗi kết nối đến máy chủ'),
  });

  const onFinish = (values: any) => {
    // Ép kiểu Date về dạng UTC ISO String gửi xuống backend
    const utcStartTime = dayjs(values.startTime).utc().format();

    const payload: CreateShowtimeRequest = {
      movieId: values.movieId,
      cinemaHallId: values.cinemaHallId,
      startTime: utcStartTime, // Ví dụ: 2026-05-01T10:00:00Z
      price: values.price,
    };

    createMutation.mutate(payload);
  };

  return (
    <Card title="Tạo Lịch Chiếu Mới" style={{ maxWidth: 600 }}>
      <Form form={form} layout="vertical" onFinish={onFinish}>
        <Form.Item 
          name="movieId" 
          label="Chọn Phim" 
          rules={[{ required: true, message: 'Vui lòng chọn phim' }]}
        >
          <Select placeholder="Chọn phim" loading={isLoadingMovies} showSearch optionFilterProp="children">
            {moviesRes?.data?.items?.map(m => (
              <Option key={m.id} value={m.id}>{m.title}</Option>
            ))}
          </Select>
        </Form.Item>

        <Form.Item 
          name="cinemaId" 
          label="Chọn Rạp" 
          rules={[{ required: true, message: 'Vui lòng chọn rạp' }]}
        >
          <Select 
            placeholder="Chọn cụm rạp" 
            loading={isLoadingCinemas}
            onChange={(val) => {
              setSelectedCinemaId(val);
              form.setFieldsValue({ cinemaHallId: undefined }); // Reset phòng chiếu
            }}
          >
            {cinemasRes?.data?.items?.map(c => (
              <Option key={c.id} value={c.id}>{c.name}</Option>
            ))}
          </Select>
        </Form.Item>

        <Form.Item 
          name="cinemaHallId" 
          label="Chọn Phòng Chiếu" 
          rules={[{ required: true, message: 'Vui lòng chọn phòng' }]}
        >
          <Select 
            placeholder="Chọn phòng chiếu" 
            loading={isLoadingHalls} 
            disabled={!selectedCinemaId}
          >
            {hallsRes?.data?.map(h => (
              <Option key={h.id} value={h.id}>{h.name} ({h.totalSeats} ghế)</Option>
            ))}
          </Select>
        </Form.Item>

        <Form.Item 
          name="startTime" 
          label="Ngày Giờ Chiếu" 
          rules={[{ required: true, message: 'Vui lòng chọn ngày giờ' }]}
        >
          <DatePicker 
            showTime 
            format="DD/MM/YYYY HH:mm" 
            style={{ width: '100%' }} 
            placeholder="Chọn ngày và giờ"
          />
        </Form.Item>

        <Form.Item 
          name="price" 
          label="Giá vé cơ bản (VNĐ)" 
          rules={[{ required: true, message: 'Vui lòng nhập giá vé' }]}
        >
          <InputNumber 
            style={{ width: '100%' }} 
            min={1000} 
            step={1000}
            formatter={value => `${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}
          />
        </Form.Item>

        <Button type="primary" htmlType="submit" loading={createMutation.isPending} block>
          Tạo Lịch Chiếu
        </Button>
      </Form>
    </Card>
  );
};
