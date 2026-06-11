import { ReloadOutlined, SearchOutlined } from '@ant-design/icons';
import { Button, Card, Input, Space, Table } from 'antd';

export default function TicketsPage() {
  return (
    <Space direction="vertical" size={20} style={{ width: '100%' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', gap: 16 }}>
        <div>
          <h1 className="page-title">Tickets</h1>
          <div className="page-subtitle">Search and check-in operations</div>
        </div>
        <Button icon={<ReloadOutlined />} />
      </div>
      <Card>
        <Input.Search
          allowClear
          enterButton={<Button type="primary" icon={<SearchOutlined />}>Search</Button>}
          style={{ maxWidth: 520, marginBottom: 16 }}
        />
        <Table
          rowKey="bookingId"
          dataSource={[]}
          columns={[
            { title: 'Ticket', dataIndex: 'ticketCode' },
            { title: 'Customer', dataIndex: 'customer' },
            { title: 'Status', dataIndex: 'status', width: 180 },
          ]}
        />
      </Card>
    </Space>
  );
}
