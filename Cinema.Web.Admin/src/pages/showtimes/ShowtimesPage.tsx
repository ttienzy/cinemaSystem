import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  CalendarOutlined,
  ClockCircleOutlined,
  PlusOutlined,
  ReloadOutlined,
} from '@ant-design/icons';
import {
  Alert,
  App,
  Button,
  Card,
  DatePicker,
  Drawer,
  Form,
  InputNumber,
  Select,
  Space,
  Table,
  Tag,
  Timeline,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import dayjs from 'dayjs';
import { cinemaApi, type CinemaHall } from '../../features/cinemas/cinemaApi';
import { movieApi } from '../../features/movies/movieApi';
import { showtimeApi, type ShowtimeLookupItem } from '../../features/showtimes/showtimeApi';
import { formatDateTime, formatMoney } from '../../shared/utils/format';

interface ShowtimeFormValues {
  movieId: string;
  cinemaId: string;
  cinemaHallId: string;
  startTime: dayjs.Dayjs;
  price: number;
}

const MIN_SHOWTIME_HOUR = 8;

export default function ShowtimesPage() {
  const { message } = App.useApp();
  const queryClient = useQueryClient();
  const [form] = Form.useForm<ShowtimeFormValues>();
  const [selectedCinemaId, setSelectedCinemaId] = useState<string | undefined>();
  const [formCinemaId, setFormCinemaId] = useState<string | undefined>();
  const [selectedDate, setSelectedDate] = useState(dayjs());
  const [drawerOpen, setDrawerOpen] = useState(false);

  const cinemasQuery = useQuery({
    queryKey: ['cinemas-for-showtime'],
    queryFn: () => cinemaApi.getCinemas(1, 100),
  });

  const moviesQuery = useQuery({
    queryKey: ['movies-for-showtime'],
    queryFn: () => movieApi.getMovies(1, 100),
  });

  const timelineHallsQuery = useQuery({
    queryKey: ['halls-for-showtime', selectedCinemaId],
    queryFn: () => cinemaApi.getHallsByCinema(selectedCinemaId!),
    enabled: !!selectedCinemaId,
  });

  const formHallsQuery = useQuery({
    queryKey: ['halls-for-showtime', formCinemaId],
    queryFn: () => cinemaApi.getHallsByCinema(formCinemaId!),
    enabled: !!formCinemaId,
  });

  const timelineQuery = useQuery({
    queryKey: ['showtimes-range', selectedCinemaId, selectedDate.format('YYYY-MM-DD')],
    queryFn: () =>
      showtimeApi.getRange(
        selectedDate.startOf('day').toISOString(),
        selectedDate.endOf('day').toISOString(),
      ),
    enabled: !!selectedCinemaId,
  });

  const createMutation = useMutation({
    mutationFn: showtimeApi.createShowtime,
    onSuccess: async () => {
      message.success('Showtime created');
      setDrawerOpen(false);
      form.resetFields();
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['showtimes-range'] }),
        queryClient.invalidateQueries({ queryKey: ['movies-admin-list'] }),
        queryClient.invalidateQueries({ queryKey: ['dashboard-summary'] }),
        queryClient.invalidateQueries({ queryKey: ['dashboard-kpi-snapshot'] }),
      ]);
    },
  });

  const hallMap = useMemo(() => {
    return new Map((timelineHallsQuery.data?.data ?? []).map((hall) => [hall.id, hall]));
  }, [timelineHallsQuery.data?.data]);

  const formHallMap = useMemo(() => {
    return new Map((formHallsQuery.data?.data ?? []).map((hall) => [hall.id, hall]));
  }, [formHallsQuery.data?.data]);

  const selectedFormHallId = Form.useWatch('cinemaHallId', form);
  const selectedFormHall = selectedFormHallId ? formHallMap.get(selectedFormHallId) : undefined;

  const hallIds = new Set((timelineHallsQuery.data?.data ?? []).map((hall) => hall.id));
  const timelineRows = (timelineQuery.data?.data ?? [])
    .filter((showtime) => hallIds.has(showtime.cinemaHallId))
    .sort((left, right) => dayjs(left.startTime).valueOf() - dayjs(right.startTime).valueOf());

  const groupedByHall = (timelineHallsQuery.data?.data ?? []).map((hall) => ({
    hall,
    showtimes: timelineRows.filter((showtime) => showtime.cinemaHallId === hall.id),
  }));

  const columns: ColumnsType<ShowtimeLookupItem> = [
    {
      title: 'Start',
      dataIndex: 'startTime',
      width: 160,
      render: (value: string) => formatDateTime(value),
    },
    {
      title: 'End',
      dataIndex: 'endTime',
      width: 160,
      render: (value: string) => formatDateTime(value),
    },
    { title: 'Movie', dataIndex: 'movieTitle' },
    {
      title: 'Hall',
      dataIndex: 'cinemaHallId',
      width: 160,
      render: (value: string) => hallMap.get(value)?.name ?? value,
    },
    {
      title: 'Price',
      dataIndex: 'price',
      width: 150,
      render: (value: number) => formatMoney(value),
    },
  ];

  const submitCreate = (values: ShowtimeFormValues) => {
    const hall = formHallMap.get(values.cinemaHallId);
    if (!hall?.seatMapConfigured) {
      message.warning('Configure seat map before creating showtime');
      return;
    }

    createMutation.mutate({
      movieId: values.movieId,
      cinemaHallId: values.cinemaHallId,
      startTime: values.startTime.toISOString(),
      price: values.price,
    });
  };

  const disabledTime = () => ({
    disabledHours: () => Array.from({ length: MIN_SHOWTIME_HOUR }, (_item, index) => index),
    disabledMinutes: (hour: number) => (hour === MIN_SHOWTIME_HOUR ? [0] : []),
  });

  return (
    <Space direction="vertical" size={20} style={{ width: '100%' }}>
      <div className="page-header">
        <div>
          <h1 className="page-title">Showtimes</h1>
          <div className="page-subtitle">Screening schedule</div>
        </div>
        <Space>
          <Button icon={<ReloadOutlined />} onClick={() => void timelineQuery.refetch()} />
          <Button type="primary" icon={<PlusOutlined />} onClick={() => setDrawerOpen(true)}>
            Showtime
          </Button>
        </Space>
      </div>

      <Card>
        <Space wrap style={{ width: '100%', marginBottom: 16 }}>
          <Select
            placeholder="Cinema"
            loading={cinemasQuery.isLoading}
            value={selectedCinemaId}
            onChange={setSelectedCinemaId}
            options={(cinemasQuery.data?.data.items ?? []).map((cinema) => ({
              label: cinema.name,
              value: cinema.id,
            }))}
            style={{ width: 260 }}
          />
          <DatePicker
            value={selectedDate}
            onChange={(value) => setSelectedDate(value ?? dayjs())}
            format="DD/MM/YYYY"
            suffixIcon={<CalendarOutlined />}
          />
        </Space>

        <Table<ShowtimeLookupItem>
          rowKey="showtimeId"
          loading={timelineQuery.isLoading || timelineHallsQuery.isLoading}
          dataSource={timelineRows}
          columns={columns}
          scroll={{ x: 820 }}
          pagination={{ pageSize: 8 }}
        />
      </Card>

      <Card title="Timeline by hall">
        {!selectedCinemaId ? (
          <Alert type="info" showIcon message="Select a cinema to view timeline" />
        ) : (
          <Space direction="vertical" size={18} style={{ width: '100%' }}>
            {groupedByHall.map(({ hall, showtimes }) => (
              <div key={hall.id} className="timeline-row">
                <div className="timeline-hall">
                  <Space direction="vertical" size={0}>
                    <strong>{hall.name}</strong>
                    <span>{hall.totalSeats} seats</span>
                  </Space>
                  <Tag color={hall.seatMapConfigured ? 'green' : 'gold'}>
                    {hall.seatMapConfigured ? 'Seat map ready' : 'No seat map'}
                  </Tag>
                </div>
                <Timeline
                  mode="left"
                  items={
                    showtimes.length
                      ? showtimes.map((showtime) => ({
                          color: 'blue',
                          dot: <ClockCircleOutlined />,
                          label: dayjs(showtime.startTime).format('HH:mm'),
                          children: (
                            <Space direction="vertical" size={0}>
                              <strong>{showtime.movieTitle}</strong>
                              <span>
                                {dayjs(showtime.startTime).format('HH:mm')} - {dayjs(showtime.endTime).format('HH:mm')}
                              </span>
                              <span>{formatMoney(showtime.price)}</span>
                            </Space>
                          ),
                        }))
                      : [{ color: 'gray', children: 'No showtime' }]
                  }
                />
              </div>
            ))}
          </Space>
        )}
      </Card>

      <Drawer
        title="Create showtime"
        width={520}
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        destroyOnHidden
        extra={
          <Button type="primary" loading={createMutation.isPending} onClick={() => form.submit()}>
            Save
          </Button>
        }
      >
        <Form form={form} layout="vertical" onFinish={submitCreate}>
          <Form.Item name="movieId" label="Movie" rules={[{ required: true }]}>
            <Select
              showSearch
              loading={moviesQuery.isLoading}
              optionFilterProp="label"
              options={(moviesQuery.data?.data.items ?? []).map((movie) => ({
                label: movie.title,
                value: movie.id,
              }))}
            />
          </Form.Item>

          <Form.Item name="cinemaId" label="Cinema" rules={[{ required: true }]}>
            <Select
              loading={cinemasQuery.isLoading}
              options={(cinemasQuery.data?.data.items ?? []).map((cinema) => ({
                label: cinema.name,
                value: cinema.id,
              }))}
              onChange={(value) => {
                setFormCinemaId(value);
                form.setFieldValue('cinemaHallId', undefined);
              }}
            />
          </Form.Item>

          <Form.Item name="cinemaHallId" label="Hall" rules={[{ required: true }]}>
            <Select
              loading={formHallsQuery.isLoading}
              disabled={!formCinemaId}
              optionRender={(option) => {
                const hall = formHallMap.get(String(option.value));
                return (
                  <Space>
                    <span>{option.label}</span>
                    {hall && <Tag color={hall.seatMapConfigured ? 'green' : 'gold'}>{hall.totalSeats} seats</Tag>}
                  </Space>
                );
              }}
              options={(formHallsQuery.data?.data ?? []).map((hall: CinemaHall) => ({
                label: hall.name,
                value: hall.id,
              }))}
            />
          </Form.Item>

          {selectedFormHall && (
            <Alert
              showIcon
              type={selectedFormHall.seatMapConfigured ? 'success' : 'warning'}
              message={
                selectedFormHall.seatMapConfigured
                  ? `Seat map ready: ${selectedFormHall.totalSeats} seats`
                  : 'Seat map is missing for this hall'
              }
              style={{ marginBottom: 16 }}
            />
          )}

          <Form.Item
            name="startTime"
            label="Start time"
            rules={[
              { required: true },
              {
                validator: (_, value: dayjs.Dayjs | undefined) => {
                  if (!value) return Promise.resolve();
                  if (!value.startOf('day').isAfter(dayjs().startOf('day'))) {
                    return Promise.reject(new Error('Start date must be after today'));
                  }
                  if (value.hour() < MIN_SHOWTIME_HOUR || (value.hour() === MIN_SHOWTIME_HOUR && value.minute() === 0)) {
                    return Promise.reject(new Error('Start time must be after 08:00'));
                  }
                  return Promise.resolve();
                },
              },
            ]}
          >
            <DatePicker
              showTime={{ format: 'HH:mm', minuteStep: 5, disabledTime }}
              disabledDate={(current) => !!current && !current.startOf('day').isAfter(dayjs().startOf('day'))}
              format="DD/MM/YYYY HH:mm"
              style={{ width: '100%' }}
            />
          </Form.Item>

          <Form.Item name="price" label="Price" rules={[{ required: true }]}>
            <InputNumber min={10000} max={10000000} step={1000} style={{ width: '100%' }} />
          </Form.Item>
        </Form>
      </Drawer>
    </Space>
  );
}
