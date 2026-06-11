import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  DeleteOutlined,
  EditOutlined,
  PlusOutlined,
  ReloadOutlined,
  SearchOutlined,
  TableOutlined,
} from '@ant-design/icons';
import {
  App,
  Button,
  Card,
  Col,
  Drawer,
  Form,
  Input,
  InputNumber,
  Modal,
  Row,
  Select,
  Space,
  Statistic,
  Table,
  Tag,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import type { Key } from 'react';
import {
  cinemaApi,
  cinemaStatuses,
  type CinemaAdminOverview,
  type CinemaHall,
  type Seat,
} from '../../features/cinemas/cinemaApi';
import { formatDateTime } from '../../shared/utils/format';

interface CinemaFormValues {
  name: string;
  address: string;
  city?: string;
}

interface HallFormValues {
  name: string;
}

interface SeatFormValues {
  row: string;
  number: number;
}

interface BulkSeatFormValues {
  rows: string;
  seatsPerRow: number;
}

export default function CinemasPage() {
  const { message, modal } = App.useApp();
  const queryClient = useQueryClient();
  const [cinemaForm] = Form.useForm<CinemaFormValues>();
  const [hallForm] = Form.useForm<HallFormValues>();
  const [seatForm] = Form.useForm<SeatFormValues>();
  const [bulkSeatForm] = Form.useForm<BulkSeatFormValues>();
  const [searchText, setSearchText] = useState('');
  const [search, setSearch] = useState('');
  const [city, setCity] = useState('');
  const [status, setStatus] = useState<string | undefined>();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [cinemaDrawerOpen, setCinemaDrawerOpen] = useState(false);
  const [hallDrawerOpen, setHallDrawerOpen] = useState(false);
  const [seatDrawerOpen, setSeatDrawerOpen] = useState(false);
  const [seatModalOpen, setSeatModalOpen] = useState(false);
  const [editingCinema, setEditingCinema] = useState<CinemaAdminOverview | null>(null);
  const [selectedCinema, setSelectedCinema] = useState<CinemaAdminOverview | null>(null);
  const [editingHall, setEditingHall] = useState<CinemaHall | null>(null);
  const [selectedHall, setSelectedHall] = useState<CinemaHall | null>(null);
  const [editingSeat, setEditingSeat] = useState<Seat | null>(null);
  const [selectedSeatIds, setSelectedSeatIds] = useState<Key[]>([]);

  const overviewQuery = useQuery({
    queryKey: ['cinemas-overview', search, city, status, page, pageSize],
    queryFn: () => cinemaApi.getOverview({ search, city, status, pageNumber: page, pageSize }),
  });

  const summaryQuery = useQuery({
    queryKey: ['cinemas-summary'],
    queryFn: cinemaApi.getSummary,
  });

  const seatsQuery = useQuery({
    queryKey: ['hall-seats', selectedHall?.id],
    queryFn: () => cinemaApi.getSeatsByHall(selectedHall!.id),
    enabled: !!selectedHall,
  });

  const invalidateCinemas = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ['cinemas-overview'] }),
      queryClient.invalidateQueries({ queryKey: ['cinemas-summary'] }),
      queryClient.invalidateQueries({ queryKey: ['cinemas-for-showtime'] }),
      queryClient.invalidateQueries({ queryKey: ['halls-for-showtime'] }),
    ]);
  };

  const invalidateSeats = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ['hall-seats'] }),
      invalidateCinemas(),
    ]);
  };

  const createCinemaMutation = useMutation({
    mutationFn: cinemaApi.createCinema,
    onSuccess: async () => {
      message.success('Cinema created');
      setCinemaDrawerOpen(false);
      await invalidateCinemas();
    },
  });

  const updateCinemaMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: CinemaFormValues }) => cinemaApi.updateCinema(id, data),
    onSuccess: async () => {
      message.success('Cinema updated');
      setCinemaDrawerOpen(false);
      await invalidateCinemas();
    },
  });

  const deleteCinemaMutation = useMutation({
    mutationFn: cinemaApi.deleteCinema,
    onSuccess: async () => {
      message.success('Cinema deleted');
      await invalidateCinemas();
    },
  });

  const createHallMutation = useMutation({
    mutationFn: ({ cinemaId, name }: { cinemaId: string; name: string }) => cinemaApi.createHall({ cinemaId, name }),
    onSuccess: async () => {
      message.success('Hall created');
      setHallDrawerOpen(false);
      await invalidateCinemas();
    },
  });

  const updateHallMutation = useMutation({
    mutationFn: ({ id, name }: { id: string; name: string }) => cinemaApi.updateHall(id, { name }),
    onSuccess: async () => {
      message.success('Hall updated');
      setHallDrawerOpen(false);
      await invalidateCinemas();
    },
  });

  const deleteHallMutation = useMutation({
    mutationFn: cinemaApi.deleteHall,
    onSuccess: async () => {
      message.success('Hall deleted');
      await invalidateCinemas();
    },
  });

  const createSeatMutation = useMutation({
    mutationFn: cinemaApi.createSeat,
    onSuccess: async () => {
      message.success('Seat created');
      setSeatModalOpen(false);
      await invalidateSeats();
    },
  });

  const updateSeatMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: SeatFormValues }) => cinemaApi.updateSeat(id, data),
    onSuccess: async () => {
      message.success('Seat updated');
      setSeatModalOpen(false);
      await invalidateSeats();
    },
  });

  const deleteSeatMutation = useMutation({
    mutationFn: cinemaApi.deleteSeat,
    onSuccess: async () => {
      message.success('Seat deleted');
      await invalidateSeats();
    },
  });

  const bulkCreateSeatMutation = useMutation({
    mutationFn: cinemaApi.bulkCreateSeats,
    onSuccess: async () => {
      message.success('Seats generated');
      bulkSeatForm.resetFields();
      await invalidateSeats();
    },
  });

  const bulkDeleteSeatMutation = useMutation({
    mutationFn: cinemaApi.bulkDeleteSeats,
    onSuccess: async () => {
      message.success('Seats deleted');
      setSelectedSeatIds([]);
      await invalidateSeats();
    },
  });

  const data = overviewQuery.data?.data;
  const summary = summaryQuery.data?.data;

  const openCreateCinema = () => {
    setEditingCinema(null);
    cinemaForm.resetFields();
    setCinemaDrawerOpen(true);
  };

  const openEditCinema = (cinema: CinemaAdminOverview) => {
    setEditingCinema(cinema);
    cinemaForm.setFieldsValue({
      name: cinema.name,
      address: cinema.address,
      city: cinema.city ?? '',
    });
    setCinemaDrawerOpen(true);
  };

  const openCreateHall = (cinema: CinemaAdminOverview) => {
    setSelectedCinema(cinema);
    setEditingHall(null);
    hallForm.resetFields();
    setHallDrawerOpen(true);
  };

  const openEditHall = (cinema: CinemaAdminOverview, hall: CinemaHall) => {
    setSelectedCinema(cinema);
    setEditingHall(hall);
    hallForm.setFieldsValue({ name: hall.name });
    setHallDrawerOpen(true);
  };

  const openSeatMap = (hall: CinemaHall) => {
    setSelectedHall(hall);
    setSelectedSeatIds([]);
    bulkSeatForm.setFieldsValue({ rows: 'A,B,C,D,E,F', seatsPerRow: 10 });
    setSeatDrawerOpen(true);
  };

  const openCreateSeat = () => {
    setEditingSeat(null);
    seatForm.resetFields();
    setSeatModalOpen(true);
  };

  const openEditSeat = (seat: Seat) => {
    setEditingSeat(seat);
    seatForm.setFieldsValue({ row: seat.row, number: seat.number });
    setSeatModalOpen(true);
  };

  const cinemaColumns = useMemo<ColumnsType<CinemaAdminOverview>>(
    () => [
      {
        title: 'Cinema',
        dataIndex: 'name',
        render: (_value, cinema) => (
          <Space direction="vertical" size={0}>
            <span style={{ fontWeight: 600 }}>{cinema.name}</span>
            <span style={{ color: '#667085' }}>{cinema.address}</span>
          </Space>
        ),
      },
      { title: 'City', dataIndex: 'city', width: 160, render: (value?: string) => value || '-' },
      {
        title: 'Status',
        dataIndex: 'status',
        width: 120,
        render: (value: string) => <Tag color={value === 'Active' ? 'green' : 'default'}>{value}</Tag>,
      },
      { title: 'Halls', dataIndex: 'totalHalls', width: 100 },
      { title: 'Seats', dataIndex: 'totalSeats', width: 100 },
      {
        title: '',
        key: 'actions',
        width: 180,
        render: (_, cinema) => (
          <Space>
            <Button icon={<PlusOutlined />} onClick={() => openCreateHall(cinema)} />
            <Button icon={<EditOutlined />} onClick={() => openEditCinema(cinema)} />
            <Button
              danger
              icon={<DeleteOutlined />}
              loading={deleteCinemaMutation.isPending}
              onClick={() =>
                modal.confirm({
                  title: 'Delete cinema',
                  content: cinema.name,
                  okButtonProps: { danger: true },
                  onOk: () => deleteCinemaMutation.mutateAsync(cinema.id),
                })
              }
            />
          </Space>
        ),
      },
    ],
    [deleteCinemaMutation, modal],
  );

  const seatColumns: ColumnsType<Seat> = [
    { title: 'Display', dataIndex: 'displayName' },
    { title: 'Row', dataIndex: 'row', width: 100 },
    { title: 'Number', dataIndex: 'number', width: 100 },
    {
      title: '',
      width: 112,
      render: (_, seat) => (
        <Space>
          <Button icon={<EditOutlined />} onClick={() => openEditSeat(seat)} />
          <Button
            danger
            icon={<DeleteOutlined />}
            onClick={() =>
              modal.confirm({
                title: 'Delete seat',
                content: seat.displayName,
                okButtonProps: { danger: true },
                onOk: () => deleteSeatMutation.mutateAsync(seat.id),
              })
            }
          />
        </Space>
      ),
    },
  ];

  const submitCinema = (values: CinemaFormValues) => {
    if (editingCinema) {
      updateCinemaMutation.mutate({ id: editingCinema.id, data: values });
    } else {
      createCinemaMutation.mutate(values);
    }
  };

  const submitHall = (values: HallFormValues) => {
    if (editingHall) {
      updateHallMutation.mutate({ id: editingHall.id, name: values.name });
      return;
    }

    if (!selectedCinema) return;
    createHallMutation.mutate({ cinemaId: selectedCinema.id, name: values.name });
  };

  const submitSeat = (values: SeatFormValues) => {
    if (!selectedHall) return;

    const data = { row: values.row.trim().toUpperCase(), number: values.number };
    if (editingSeat) {
      updateSeatMutation.mutate({ id: editingSeat.id, data });
    } else {
      createSeatMutation.mutate({ cinemaHallId: selectedHall.id, ...data });
    }
  };

  const submitBulkSeats = (values: BulkSeatFormValues) => {
    if (!selectedHall) return;

    const rows = values.rows
      .split(',')
      .map((row) => row.trim().toUpperCase())
      .filter(Boolean);

    const seats = rows.flatMap((row) =>
      Array.from({ length: values.seatsPerRow }, (_, index) => ({
        row,
        number: index + 1,
      })),
    );

    bulkCreateSeatMutation.mutate({ cinemaHallId: selectedHall.id, seats });
  };

  return (
    <Space direction="vertical" size={20} style={{ width: '100%' }}>
      <div className="page-header">
        <div>
          <h1 className="page-title">Cinemas</h1>
          <div className="page-subtitle">Locations, halls, and seat maps</div>
        </div>
        <Space>
          <Button icon={<ReloadOutlined />} onClick={() => void overviewQuery.refetch()} />
          <Button type="primary" icon={<PlusOutlined />} onClick={openCreateCinema}>
            Cinema
          </Button>
        </Space>
      </div>

      <Row gutter={[16, 16]}>
        <Col xs={12} lg={6}>
          <Card className="metric-card" loading={summaryQuery.isLoading}>
            <Statistic title="Total" value={summary?.totalCinemas ?? 0} />
          </Card>
        </Col>
        <Col xs={12} lg={6}>
          <Card className="metric-card" loading={summaryQuery.isLoading}>
            <Statistic title="Active" value={summary?.activeCinemas ?? 0} />
          </Card>
        </Col>
        <Col xs={12} lg={6}>
          <Card className="metric-card" loading={summaryQuery.isLoading}>
            <Statistic title="Halls" value={summary?.totalHalls ?? 0} />
          </Card>
        </Col>
        <Col xs={12} lg={6}>
          <Card className="metric-card" loading={summaryQuery.isLoading}>
            <Statistic title="Seats" value={summary?.totalSeats ?? 0} />
          </Card>
        </Col>
      </Row>

      <Card>
        <Space wrap style={{ width: '100%', marginBottom: 16 }}>
          <Input
            allowClear
            prefix={<SearchOutlined />}
            placeholder="Search cinema"
            value={searchText}
            onChange={(event) => setSearchText(event.target.value)}
            onPressEnter={() => {
              setPage(1);
              setSearch(searchText);
            }}
            style={{ width: 240 }}
          />
          <Input
            allowClear
            placeholder="City"
            value={city}
            onChange={(event) => setCity(event.target.value)}
            style={{ width: 180 }}
          />
          <Select
            allowClear
            placeholder="Status"
            value={status}
            onChange={(value) => {
              setPage(1);
              setStatus(value);
            }}
            options={cinemaStatuses.map((item) => ({ label: item, value: item }))}
            style={{ width: 160 }}
          />
          <Button
            icon={<SearchOutlined />}
            onClick={() => {
              setPage(1);
              setSearch(searchText);
            }}
          >
            Search
          </Button>
        </Space>

        <Table<CinemaAdminOverview>
          rowKey="id"
          loading={overviewQuery.isLoading}
          dataSource={data?.items ?? []}
          columns={cinemaColumns}
          scroll={{ x: 840 }}
          expandable={{
            expandedRowRender: (cinema) => (
              <Table<CinemaHall>
                size="small"
                rowKey="id"
                pagination={false}
                dataSource={cinema.cinemaHalls}
                columns={[
                  { title: 'Hall', dataIndex: 'name' },
                  { title: 'Seats', dataIndex: 'totalSeats', width: 100 },
                  {
                    title: 'Seat map',
                    dataIndex: 'seatMapConfigured',
                    width: 140,
                    render: (value: boolean) => <Tag color={value ? 'green' : 'gold'}>{value ? 'Ready' : 'Missing'}</Tag>,
                  },
                  {
                    title: 'Created',
                    dataIndex: 'createdAt',
                    width: 160,
                    render: (value: string) => formatDateTime(value),
                  },
                  {
                    title: '',
                    width: 180,
                    render: (_, hall) => (
                      <Space>
                        <Button icon={<TableOutlined />} onClick={() => openSeatMap(hall)} />
                        <Button icon={<EditOutlined />} onClick={() => openEditHall(cinema, hall)} />
                        <Button
                          danger
                          icon={<DeleteOutlined />}
                          loading={deleteHallMutation.isPending}
                          onClick={() =>
                            modal.confirm({
                              title: 'Delete hall',
                              content: hall.name,
                              okButtonProps: { danger: true },
                              onOk: () => deleteHallMutation.mutateAsync(hall.id),
                            })
                          }
                        />
                      </Space>
                    ),
                  },
                ]}
              />
            ),
          }}
          pagination={{
            current: page,
            pageSize,
            total: data?.totalCount ?? 0,
            showSizeChanger: true,
            onChange: (nextPage, nextSize) => {
              setPage(nextPage);
              setPageSize(nextSize);
            },
          }}
        />
      </Card>

      <Drawer
        title={editingCinema ? 'Edit cinema' : 'Create cinema'}
        width={480}
        open={cinemaDrawerOpen}
        onClose={() => setCinemaDrawerOpen(false)}
        destroyOnHidden
        extra={
          <Button
            type="primary"
            loading={createCinemaMutation.isPending || updateCinemaMutation.isPending}
            onClick={() => cinemaForm.submit()}
          >
            Save
          </Button>
        }
      >
        <Form form={cinemaForm} layout="vertical" onFinish={submitCinema}>
          <Form.Item name="name" label="Name" rules={[{ required: true }]}>
            <Input maxLength={200} />
          </Form.Item>
          <Form.Item name="address" label="Address" rules={[{ required: true }]}>
            <Input.TextArea rows={3} maxLength={500} />
          </Form.Item>
          <Form.Item name="city" label="City">
            <Input maxLength={100} />
          </Form.Item>
        </Form>
      </Drawer>

      <Drawer
        title={editingHall ? 'Edit hall' : 'Create hall'}
        width={420}
        open={hallDrawerOpen}
        onClose={() => setHallDrawerOpen(false)}
        destroyOnHidden
        extra={
          <Button
            type="primary"
            loading={createHallMutation.isPending || updateHallMutation.isPending}
            onClick={() => hallForm.submit()}
          >
            Save
          </Button>
        }
      >
        <Form form={hallForm} layout="vertical" onFinish={submitHall}>
          <Form.Item label="Cinema">
            <Input value={selectedCinema?.name} disabled />
          </Form.Item>
          <Form.Item name="name" label="Hall name" rules={[{ required: true }]}>
            <Input maxLength={100} />
          </Form.Item>
        </Form>
      </Drawer>

      <Drawer
        title={selectedHall ? `Seat map: ${selectedHall.name}` : 'Seat map'}
        width={720}
        open={seatDrawerOpen}
        onClose={() => setSeatDrawerOpen(false)}
        destroyOnHidden
        extra={
          <Button type="primary" icon={<PlusOutlined />} onClick={openCreateSeat}>
            Seat
          </Button>
        }
      >
        <Space direction="vertical" size={16} style={{ width: '100%' }}>
          <Form form={bulkSeatForm} layout="inline" onFinish={submitBulkSeats}>
            <Form.Item name="rows" label="Rows" rules={[{ required: true }]}>
              <Input placeholder="A,B,C" style={{ width: 180 }} />
            </Form.Item>
            <Form.Item name="seatsPerRow" label="Seats/row" rules={[{ required: true }]}>
              <InputNumber min={1} max={100} />
            </Form.Item>
            <Button type="primary" htmlType="submit" loading={bulkCreateSeatMutation.isPending}>
              Generate
            </Button>
            <Button
              danger
              disabled={selectedSeatIds.length === 0}
              loading={bulkDeleteSeatMutation.isPending}
              onClick={() =>
                modal.confirm({
                  title: 'Delete selected seats',
                  content: `${selectedSeatIds.length} seat(s)`,
                  okButtonProps: { danger: true },
                  onOk: () => bulkDeleteSeatMutation.mutateAsync(selectedSeatIds.map(String)),
                })
              }
            >
              Delete selected
            </Button>
          </Form>

          <Table<Seat>
            size="small"
            rowKey="id"
            loading={seatsQuery.isLoading}
            dataSource={(seatsQuery.data?.data ?? []).sort((left, right) =>
              `${left.row}${left.number.toString().padStart(3, '0')}`.localeCompare(
                `${right.row}${right.number.toString().padStart(3, '0')}`,
              ),
            )}
            rowSelection={{
              selectedRowKeys: selectedSeatIds,
              onChange: setSelectedSeatIds,
            }}
            columns={seatColumns}
            pagination={{ pageSize: 12 }}
          />
        </Space>
      </Drawer>

      <Modal
        title={editingSeat ? 'Edit seat' : 'Create seat'}
        open={seatModalOpen}
        onCancel={() => setSeatModalOpen(false)}
        onOk={() => seatForm.submit()}
        confirmLoading={createSeatMutation.isPending || updateSeatMutation.isPending}
      >
        <Form form={seatForm} layout="vertical" onFinish={submitSeat}>
          <Row gutter={12}>
            <Col span={12}>
              <Form.Item name="row" label="Row" rules={[{ required: true }]}>
                <Input maxLength={10} />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="number" label="Number" rules={[{ required: true }]}>
                <InputNumber min={1} max={100} style={{ width: '100%' }} />
              </Form.Item>
            </Col>
          </Row>
        </Form>
      </Modal>
    </Space>
  );
}
