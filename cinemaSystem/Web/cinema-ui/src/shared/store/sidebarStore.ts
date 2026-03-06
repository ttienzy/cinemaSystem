import { create } from 'zustand';
import { persist } from 'zustand/middleware';

interface SidebarState {
    collapsed: boolean;
    isMobile: boolean;
    setCollapsed: (collapsed: boolean) => void;
    toggleCollapsed: () => void;
    setIsMobile: (isMobile: boolean) => void;
}

export const useSidebarStore = create<SidebarState>()(
    persist(
        (set) => ({
            collapsed: false,
            isMobile: false,

            setCollapsed: (collapsed: boolean) => {
                set({ collapsed });
            },

            toggleCollapsed: () => {
                set((state) => ({ collapsed: !state.collapsed }));
            },

            setIsMobile: (isMobile: boolean) => {
                set({ isMobile });
                // When switching to mobile, collapse sidebar
                if (isMobile) {
                    set({ collapsed: true });
                }
            },
        }),
        {
            name: 'cinema-sidebar-storage',
            partialize: (state): Pick<SidebarState, 'collapsed'> => ({
                collapsed: state.collapsed,
            }),
        }
    )
);

// Custom hook for using sidebar state with localStorage persistence
export const useSidebarState = () => {
    const collapsed = useSidebarStore((state) => state.collapsed);
    const isMobile = useSidebarStore((state) => state.isMobile);
    const setCollapsed = useSidebarStore((state) => state.setCollapsed);
    const toggleCollapsed = useSidebarStore((state) => state.toggleCollapsed);
    const setIsMobile = useSidebarStore((state) => state.setIsMobile);

    return {
        collapsed,
        isMobile,
        setCollapsed,
        toggleCollapsed,
        setIsMobile,
    };
};

export default useSidebarStore;
