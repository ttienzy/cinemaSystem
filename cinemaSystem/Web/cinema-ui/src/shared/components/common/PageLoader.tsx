import { Spin } from 'antd';
import { LoadingOutlined } from '@ant-design/icons';

const PageLoader = () => {
    return (
        <div style={{
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            minHeight: '200px',
            width: '100%',
        }}>
            <Spin indicator={<LoadingOutlined style={{ fontSize: 36 }} spin />} />
        </div>
    );
};

export default PageLoader;
