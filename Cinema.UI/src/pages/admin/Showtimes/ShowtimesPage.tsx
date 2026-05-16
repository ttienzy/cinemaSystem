import React from 'react';
import { Tabs } from 'antd';
import { CalendarOutlined, PlusCircleOutlined } from '@ant-design/icons';
import { CreateShowtimeForm } from '../../../features/showtimes/components/CreateShowtimeForm';
import { SeatMapEditor } from '../../../features/cinemas/components/SeatMapEditor';
import { lazy, Suspense } from 'react';
import { Spin } from 'antd';

const ShowtimeTimelinePage = lazy(() => import('./ShowtimeTimelinePage'));

const ShowtimesPage: React.FC = () => {
  return (
    <div>
      <h2 style={{ marginBottom: 16 }}>Quản lý Lịch chiếu & Phòng chiếu</h2>

      <Tabs
        defaultActiveKey="timeline"
        items={[
          {
            key: 'timeline',
            label: <span><CalendarOutlined /> Timeline View</span>,
            children: (
              <Suspense fallback={<Spin size="large" />}>
                <ShowtimeTimelinePage />
              </Suspense>
            ),
          },
          {
            key: 'create',
            label: <span><PlusCircleOutlined /> Tạo suất chiếu</span>,
            children: (
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 24 }}>
                <CreateShowtimeForm />
                <SeatMapEditor />
              </div>
            ),
          },
        ]}
      />
    </div>
  );
};

export default ShowtimesPage;
