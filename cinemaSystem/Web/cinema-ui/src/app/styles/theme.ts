import type { ThemeConfig } from 'antd';

// Cinema-themed dark/red color scheme
export const theme: ThemeConfig = {
    token: {
        // Primary colors - Cinema red theme
        colorPrimary: '#e50914', // Netflix-like red
        colorPrimaryHover: '#f40612',
        colorPrimaryActive: '#b2070f',
        colorPrimaryBg: '#2a0a0a',

        // Background colors
        colorBgBase: '#141414',
        colorBgContainer: '#1f1f1f',
        colorBgElevated: '#262626',
        colorBgLayout: '#0a0a0a',
        colorBgSpotlight: '#000000',

        // Text colors
        colorText: '#ffffff',
        colorTextSecondary: '#a3a3a3',
        colorTextTertiary: '#737373',
        colorTextQuaternary: '#525252',

        // Border colors
        colorBorder: '#333333',
        colorBorderSecondary: '#262626',

        // Success/Error/Warning/Info
        colorSuccess: '#52c41a',
        colorError: '#ff4d4f',
        colorWarning: '#faad14',
        colorInfo: '#177ddc',

        // Typography
        fontFamily: "'Inter', 'Segoe UI', -apple-system, BlinkMacSystemFont, sans-serif",
        fontSize: 14,
        fontSizeHeading1: 32,
        fontSizeHeading2: 24,
        fontSizeHeading3: 20,
        fontSizeHeading4: 16,
        fontSizeHeading5: 14,

        // Border radius
        borderRadius: 8,
        borderRadiusLG: 12,
        borderRadiusSM: 4,

        // Motion
        motionDurationFast: '0.1s',
        motionDurationMid: '0.2s',
        motionDurationSlow: '0.3s',

        // Shadows
        boxShadow: '0 4px 12px rgba(0, 0, 0, 0.4)',
        boxShadowSecondary: '0 2px 8px rgba(0, 0, 0, 0.3)',

        // Line heights
        lineHeight: 1.5714,
        lineHeightHeading1: 1.25,
        lineHeightHeading2: 1.35,
        lineHeightHeading3: 1.4,
    },
    components: {
        Layout: {
            headerBg: '#000000',
            siderBg: '#141414',
            bodyBg: '#0a0a0a',
            headerPadding: '0 24px',
        },
        Menu: {
            darkItemBg: '#141414',
            darkItemSelectedBg: '#e50914',
            darkItemHoverBg: '#262626',
            darkItemColor: '#a3a3a3',
            darkItemSelectedColor: '#ffffff',
        },
        Card: {
            colorBgContainer: '#1f1f1f',
            colorBorderSecondary: '#333333',
        },
        Button: {
            primaryShadow: '0 2px 8px rgba(229, 9, 20, 0.4)',
        },
        Input: {
            colorBgContainer: '#262626',
            colorBorder: '#333333',
            activeBorderColor: '#e50914',
            hoverBorderColor: '#525252',
        },
        Select: {
            colorBgContainer: '#262626',
            colorBorder: '#333333',
            optionSelectedBg: '#e50914',
        },
        Table: {
            colorBgContainer: '#1f1f1f',
            headerBg: '#262626',
            rowHoverBg: '#262626',
        },
        Modal: {
            contentBg: '#1f1f1f',
            headerBg: '#1f1f1f',
        },
        Drawer: {
            colorBgElevated: '#1f1f1f',
        },
        Tabs: {
            inkBarColor: '#e50914',
            itemActiveColor: '#e50914',
            itemSelectedColor: '#e50914',
            itemHoverColor: '#f40612',
        },
        Steps: {
            colorPrimary: '#e50914',
        },
        Badge: {
            colorBgContainer: '#e50914',
        },
        Tag: {
            colorFillSecondary: '#262626',
        },
        Pagination: {
            itemActiveBg: '#e50914',
        },
        DatePicker: {
            colorBgContainer: '#262626',
            colorBgElevated: '#1f1f1f',
        },
        Notification: {
            colorBgElevated: '#1f1f1f',
        },
        Message: {
            contentBg: '#1f1f1f',
        },
    },
};

export default theme;
