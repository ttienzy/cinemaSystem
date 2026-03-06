// Theme Provider - Ant Design ConfigProvider + App wrapper
import { App as AntApp, ConfigProvider } from 'antd';
import type { ReactNode } from 'react';
import { theme } from '../styles/theme';

interface ThemeProviderProps {
    children: ReactNode;
}

export function ThemeProvider({ children }: ThemeProviderProps) {
    return (
        <ConfigProvider theme={theme}>
            <AntApp>
                {children}
            </AntApp>
        </ConfigProvider>
    );
}

export default ThemeProvider;
