import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  DeleteOutlined,
  EditOutlined,
  PlusOutlined,
  ReloadOutlined,
  SearchOutlined,
  UploadOutlined,
} from '@ant-design/icons';
import {
  App,
  Button,
  Card,
  Col,
  DatePicker,
  Drawer,
  Form,
  Image,
  Input,
  InputNumber,
  Row,
  Select,
  Space,
  Statistic,
  Table,
  Tag,
  Upload,
} from 'antd';
import type { ColumnsType } from 'antd/es/table';
import type { UploadFile } from 'antd/es/upload/interface';
import dayjs from 'dayjs';
import {
  movieApi,
  movieStatuses,
  type MovieAdminListItem,
} from '../../features/movies/movieApi';
import { formatDate, formatDateTime } from '../../shared/utils/format';

interface MovieFormValues {
  title: string;
  description?: string;
  duration: number;
  language?: string;
  releaseDate: dayjs.Dayjs;
  genreIds?: string[];
  poster?: UploadFile[];
  removePoster?: boolean;
}

const statusColor: Record<string, string> = {
  Showing: 'green',
  ComingSoon: 'gold',
  Archived: 'default',
};

function normalizeUpload(value: { fileList?: UploadFile[] } | UploadFile[] | undefined): UploadFile[] {
  if (Array.isArray(value)) return value;
  return value?.fileList ?? [];
}

function buildMovieFormData(values: MovieFormValues): FormData {
  const formData = new FormData();
  formData.append('Title', values.title);
  formData.append('Description', values.description ?? '');
  formData.append('Duration', String(values.duration));
  formData.append('Language', values.language ?? '');
  formData.append('ReleaseDate', values.releaseDate.toISOString());
  formData.append('RemovePoster', String(values.removePoster ?? false));

  values.genreIds?.forEach((genreId) => formData.append('GenreIds', genreId));

  const posterFile = values.poster?.[0]?.originFileObj;
  if (posterFile) formData.append('PosterFile', posterFile);

  return formData;
}

export default function MoviesPage() {
  const { message, modal } = App.useApp();
  const queryClient = useQueryClient();
  const [form] = Form.useForm<MovieFormValues>();
  const [searchText, setSearchText] = useState('');
  const [search, setSearch] = useState('');
  const [status, setStatus] = useState<string | undefined>();
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [editingMovie, setEditingMovie] = useState<MovieAdminListItem | null>(null);

  const listQuery = useQuery({
    queryKey: ['movies-admin-list', search, status, page, pageSize],
    queryFn: () => movieApi.getAdminList({ search, status, pageNumber: page, pageSize }),
  });

  const summaryQuery = useQuery({
    queryKey: ['movies-admin-summary'],
    queryFn: movieApi.getAdminSummary,
  });

  const genresQuery = useQuery({
    queryKey: ['genres'],
    queryFn: movieApi.getGenres,
  });

  const invalidateMovies = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ['movies-admin-list'] }),
      queryClient.invalidateQueries({ queryKey: ['movies-admin-summary'] }),
      queryClient.invalidateQueries({ queryKey: ['movies-for-showtime'] }),
    ]);
  };

  const createMutation = useMutation({
    mutationFn: movieApi.createMovie,
    onSuccess: async () => {
      message.success('Movie created');
      setDrawerOpen(false);
      await invalidateMovies();
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, formData }: { id: string; formData: FormData }) => movieApi.updateMovie(id, formData),
    onSuccess: async () => {
      message.success('Movie updated');
      setDrawerOpen(false);
      setEditingMovie(null);
      await invalidateMovies();
    },
  });

  const deleteMutation = useMutation({
    mutationFn: movieApi.deleteMovie,
    onSuccess: async () => {
      message.success('Movie deleted');
      await invalidateMovies();
    },
  });

  const summary = summaryQuery.data?.data;
  const data = listQuery.data?.data;

  const openCreate = () => {
    setEditingMovie(null);
    form.resetFields();
    form.setFieldsValue({ duration: 90, releaseDate: dayjs(), poster: [] });
    setDrawerOpen(true);
  };

  const openEdit = (movie: MovieAdminListItem) => {
    setEditingMovie(movie);
    form.setFieldsValue({
      title: movie.title,
      description: movie.description ?? '',
      duration: movie.duration,
      language: movie.language ?? '',
      releaseDate: dayjs(movie.releaseDate),
      genreIds: movie.genres.map((genre) => genre.id),
      removePoster: false,
      poster: [],
    });
    setDrawerOpen(true);
  };

  const columns = useMemo<ColumnsType<MovieAdminListItem>>(
    () => [
      {
        title: 'Movie',
        dataIndex: 'title',
        render: (_value, movie) => (
          <Space>
            {movie.posterUrl ? (
              <Image src={movie.posterUrl} width={44} height={64} style={{ objectFit: 'cover', borderRadius: 4 }} />
            ) : (
              <div className="poster-placeholder">No poster</div>
            )}
            <Space direction="vertical" size={0}>
              <span style={{ fontWeight: 600 }}>{movie.title}</span>
              <span style={{ color: '#667085' }}>{movie.language || '-'}</span>
            </Space>
          </Space>
        ),
      },
      {
        title: 'Status',
        dataIndex: 'status',
        width: 140,
        render: (value: string) => <Tag color={statusColor[value] ?? 'default'}>{value}</Tag>,
      },
      { title: 'Duration', dataIndex: 'duration', width: 120, render: (value: number) => `${value} min` },
      { title: 'Release', dataIndex: 'releaseDate', width: 140, render: (value: string) => formatDate(value) },
      { title: 'Showtimes', dataIndex: 'totalShowtimes', width: 120 },
      {
        title: 'Next',
        dataIndex: 'nextShowtimeAt',
        width: 170,
        render: (value?: string | null) => formatDateTime(value),
      },
      {
        title: '',
        key: 'actions',
        width: 112,
        render: (_, movie) => (
          <Space>
            <Button icon={<EditOutlined />} onClick={() => openEdit(movie)} />
            <Button
              danger
              icon={<DeleteOutlined />}
              loading={deleteMutation.isPending}
              onClick={() =>
                modal.confirm({
                  title: 'Delete movie',
                  content: movie.title,
                  okButtonProps: { danger: true },
                  onOk: () => deleteMutation.mutateAsync(movie.id),
                })
              }
            />
          </Space>
        ),
      },
    ],
    [deleteMutation, modal],
  );

  const submitForm = async (values: MovieFormValues) => {
    const formData = buildMovieFormData(values);
    if (editingMovie) {
      updateMutation.mutate({ id: editingMovie.id, formData });
    } else {
      createMutation.mutate(formData);
    }
  };

  return (
    <Space direction="vertical" size={20} style={{ width: '100%' }}>
      <div className="page-header">
        <div>
          <h1 className="page-title">Movies</h1>
          <div className="page-subtitle">Catalog management</div>
        </div>
        <Space>
          <Button icon={<ReloadOutlined />} onClick={() => void listQuery.refetch()} />
          <Button type="primary" icon={<PlusOutlined />} onClick={openCreate}>
            Movie
          </Button>
        </Space>
      </div>

      <Row gutter={[16, 16]}>
        <Col xs={12} lg={6}>
          <Card className="metric-card" loading={summaryQuery.isLoading}>
            <Statistic title="Total" value={summary?.totalMovies ?? 0} />
          </Card>
        </Col>
        <Col xs={12} lg={6}>
          <Card className="metric-card" loading={summaryQuery.isLoading}>
            <Statistic title="Showing" value={summary?.showingMovies ?? 0} />
          </Card>
        </Col>
        <Col xs={12} lg={6}>
          <Card className="metric-card" loading={summaryQuery.isLoading}>
            <Statistic title="Coming soon" value={summary?.comingSoonMovies ?? 0} />
          </Card>
        </Col>
        <Col xs={12} lg={6}>
          <Card className="metric-card" loading={summaryQuery.isLoading}>
            <Statistic title="Archived" value={summary?.archivedMovies ?? 0} />
          </Card>
        </Col>
      </Row>

      <Card>
        <Space wrap style={{ width: '100%', marginBottom: 16 }}>
          <Input
            allowClear
            prefix={<SearchOutlined />}
            placeholder="Search title"
            value={searchText}
            onChange={(event) => setSearchText(event.target.value)}
            onPressEnter={() => {
              setPage(1);
              setSearch(searchText);
            }}
            style={{ width: 260 }}
          />
          <Select
            allowClear
            placeholder="Status"
            value={status}
            onChange={(value) => {
              setPage(1);
              setStatus(value);
            }}
            options={movieStatuses.map((item) => ({ label: item, value: item }))}
            style={{ width: 180 }}
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

        <Table<MovieAdminListItem>
          rowKey="id"
          loading={listQuery.isLoading}
          dataSource={data?.items ?? []}
          columns={columns}
          scroll={{ x: 920 }}
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
        title={editingMovie ? 'Edit movie' : 'Create movie'}
        width={520}
        open={drawerOpen}
        onClose={() => setDrawerOpen(false)}
        destroyOnHidden
        extra={
          <Button
            type="primary"
            loading={createMutation.isPending || updateMutation.isPending}
            onClick={() => form.submit()}
          >
            Save
          </Button>
        }
      >
        <Form form={form} layout="vertical" onFinish={submitForm}>
          <Form.Item name="title" label="Title" rules={[{ required: true }]}>
            <Input maxLength={200} />
          </Form.Item>
          <Form.Item name="description" label="Description">
            <Input.TextArea rows={4} />
          </Form.Item>
          <Row gutter={12}>
            <Col span={12}>
              <Form.Item name="duration" label="Duration" rules={[{ required: true }]}>
                <InputNumber min={1} max={500} style={{ width: '100%' }} />
              </Form.Item>
            </Col>
            <Col span={12}>
              <Form.Item name="language" label="Language">
                <Input maxLength={50} />
              </Form.Item>
            </Col>
          </Row>
          <Form.Item name="releaseDate" label="Release date" rules={[{ required: true }]}>
            <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
          </Form.Item>
          <Form.Item name="genreIds" label="Genres">
            <Select
              mode="multiple"
              loading={genresQuery.isLoading}
              options={(genresQuery.data?.data ?? []).map((genre) => ({ label: genre.name, value: genre.id }))}
            />
          </Form.Item>
          <Form.Item name="poster" label="Poster" valuePropName="fileList" getValueFromEvent={normalizeUpload}>
            <Upload beforeUpload={() => false} maxCount={1} accept="image/*" listType="picture">
              <Button icon={<UploadOutlined />}>Upload</Button>
            </Upload>
          </Form.Item>
          {editingMovie?.posterUrl && (
            <Form.Item name="removePoster" label="Current poster">
              <Select
                options={[
                  { label: 'Keep current poster', value: false },
                  { label: 'Remove current poster', value: true },
                ]}
              />
            </Form.Item>
          )}
        </Form>
      </Drawer>
    </Space>
  );
}
