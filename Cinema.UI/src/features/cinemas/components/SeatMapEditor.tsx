import React, { useState } from 'react';
import { Card, Button, message, Tag } from 'antd';
import { SaveOutlined } from '@ant-design/icons';

type SeatType = 'Normal' | 'VIP' | 'Couple' | 'None';

interface SeatNode {
  id: string;
  row: string;
  col: number;
  type: SeatType;
  span: number;
  isHidden: boolean;
}

const getNextType = (current: SeatType): SeatType => {
  const flow: SeatType[] = ['Normal', 'VIP', 'Couple', 'None'];
  const idx = flow.indexOf(current);
  return flow[(idx + 1) % flow.length];
};

const getSeatColor = (type: SeatType) => {
  switch (type) {
    case 'VIP': return 'gold';
    case 'Couple': return 'pink';
    case 'Normal': return '#ccc';
    default: return 'transparent';
  }
};

export const SeatMapEditor: React.FC = () => {
  const rows = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H'];
  const cols = 12;

  const [seats, setSeats] = useState<SeatNode[]>(() => {
    const initial: SeatNode[] = [];
    rows.forEach(r => {
      for (let c = 1; c <= cols; c++) {
        initial.push({ id: `${r}${c}`, row: r, col: c, type: 'Normal', span: 1, isHidden: false });
      }
    });
    return initial;
  });

  const toggleSeatType = (id: string, row: string, col: number) => {
    setSeats(prev => {
      const newSeats = [...prev];
      const seatIndex = newSeats.findIndex(s => s.id === id);
      if (seatIndex === -1) return prev;

      const currentSeat = newSeats[seatIndex];
      const nextType = getNextType(currentSeat.type);

      // Xử lý logic Couple (chiếm 2 ô)
      if (nextType === 'Couple') {
        if (col === cols) {
          message.warning('Ghế Couple không thể đặt ở cột cuối cùng!');
          return prev; // Giữ nguyên
        }
        // Kiểm tra xem ô kế tiếp có phải là đầu của một couple khác không
        const nextSeatIndex = newSeats.findIndex(s => s.row === row && s.col === col + 1);
        if (nextSeatIndex !== -1 && newSeats[nextSeatIndex].type === 'Couple') {
             message.warning('Không thể chồng ghế Couple lên nhau!');
             return prev;
        }

        // Đánh dấu ô hiện tại là Couple (span 2)
        newSeats[seatIndex] = { ...currentSeat, type: nextType, span: 2 };
        // Ẩn ô kế tiếp
        if (nextSeatIndex !== -1) {
          newSeats[nextSeatIndex] = { ...newSeats[nextSeatIndex], isHidden: true, type: 'None' };
        }
      } else {
        // Nếu chuyển từ Couple sang loại khác, hoặc từ loại khác sang loại khác
        const wasCouple = currentSeat.type === 'Couple';
        newSeats[seatIndex] = { ...currentSeat, type: nextType, span: 1 };
        
        if (wasCouple) {
          // Phục hồi lại ô kế tiếp
          const nextSeatIndex = newSeats.findIndex(s => s.row === row && s.col === col + 1);
          if (nextSeatIndex !== -1) {
            newSeats[nextSeatIndex] = { ...newSeats[nextSeatIndex], isHidden: false, type: 'Normal' };
          }
        }
      }

      return newSeats;
    });
  };

  const handleSave = () => {
    // Dựa vào Domain BE Cinema.API: Normal = 0, VIP = 1, Couple = 2
    const typeMapping: Record<string, number> = {
      'Normal': 0,
      'VIP': 1,
      'Couple': 2
    };

    const validSeats = seats
      .filter(s => s.type !== 'None' && !s.isHidden)
      .map(s => ({
        row: s.row,
        number: s.col,
        seatType: typeMapping[s.type], // Đã map chuẩn thành int
        displayName: s.id
      }));

    console.log('Payload gửi xuống Backend:', validSeats);
    message.success('Đã cấu hình Payload sơ đồ ghế thành công! (Xem Console)');
  };

  return (
    <Card 
      title="Thiết Kế Sơ Đồ Phòng Chiếu" 
      style={{ marginTop: 24, overflowX: 'auto' }}
      extra={<Button type="primary" icon={<SaveOutlined />} onClick={handleSave}>Lưu Sơ Đồ</Button>}
    >
      <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
        <div style={{ 
          width: '80%', height: 20, background: '#1677ff', 
          borderRadius: '50% 50% 0 0', textAlign: 'center', 
          color: 'white', marginBottom: 32, fontWeight: 'bold' 
        }}>
          MÀN HÌNH
        </div>
        
        <div style={{ 
          display: 'grid', 
          gridTemplateColumns: `30px repeat(${cols}, 32px) 30px`, 
          gap: '8px',
          justifyContent: 'center',
          alignItems: 'center'
        }}>
          {rows.map(row => {
            const rowSeats = seats.filter(s => s.row === row);
            
            return (
              <React.Fragment key={row}>
                {/* Header row bên trái */}
                <div style={{ fontWeight: 'bold', textAlign: 'center' }}>{row}</div>
                
                {/* Render các ghế trong hàng */}
                {rowSeats.map(seat => {
                  if (seat.isHidden) return null; // Bỏ qua không render ô bị ẩn

                  return (
                    <div 
                      key={seat.id}
                      onClick={() => toggleSeatType(seat.id, seat.row, seat.col)}
                      style={{
                        gridColumn: `span ${seat.span}`,
                        visibility: seat.type === 'None' ? 'hidden' : 'visible',
                        backgroundColor: getSeatColor(seat.type),
                        height: 32,
                        borderRadius: seat.type === 'Couple' ? 8 : 4,
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                        cursor: 'pointer',
                        fontSize: 12,
                        fontWeight: 'bold',
                        boxShadow: seat.type !== 'None' ? '0 2px 4px rgba(0,0,0,0.1)' : 'none',
                      }}
                    >
                      {seat.type === 'Couple' ? '♡' : seat.col}
                    </div>
                  );
                })}

                {/* Header row bên phải */}
                <div style={{ fontWeight: 'bold', textAlign: 'center' }}>{row}</div>
              </React.Fragment>
            );
          })}
        </div>
      </div>
      
      <div style={{ marginTop: 32, textAlign: 'center', display: 'flex', justifyContent: 'center', gap: 16 }}>
         <Tag color="#ccc" style={{ padding: '4px 16px', fontSize: 14 }}>Ghế Thường</Tag>
         <Tag color="gold" style={{ padding: '4px 16px', fontSize: 14 }}>Ghế VIP</Tag>
         <Tag color="pink" style={{ padding: '4px 16px', fontSize: 14 }}>Ghế Couple</Tag>
         <Tag color="transparent" style={{ border: '1px dashed #d9d9d9', padding: '4px 16px', fontSize: 14 }}>Lối Đi</Tag>
      </div>
    </Card>
  );
};
