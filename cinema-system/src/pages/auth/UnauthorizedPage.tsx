import React from 'react';
import { Link } from 'react-router-dom';

export const UnauthorizedPage: React.FC = () => {
  
  return (
    <div style={{ 
      textAlign: 'center'
    }}>
      <h1>Không có quyền truy cập</h1>
      <p>Bạn không có quyền truy cập vào trang này.</p>
      <Link 
        to="/"
        style={{
          textDecoration: 'none'
        }}
      >
        Quay về trang chủ
      </Link>
    </div>
  );
};