import React, { useState, useMemo, useEffect } from "react";
import { NavLink, useNavigate } from "react-router-dom";
import { Search, User, Menu, X, Film } from "lucide-react";
import { jwtDecode } from "jwt-decode";

// --- Type Imports ---
import type { DecodedToken } from "../../types/auth.types";

// --- UI Component Imports (assuming you have these) ---
import { Button } from "../ui/Button";
import { Input } from "../ui/Input";

// =================================================================
// Reusable Authentication Buttons Component
// This avoids code duplication for desktop and mobile views.
// =================================================================
const AuthButtons: React.FC<{ decoded: DecodedToken | null; isMobile?: boolean }> = ({ decoded, isMobile = false }) => {
  const navigate = useNavigate();
  const commonClasses = "w-full justify-center text-sm"; // Common classes for mobile buttons

  if (decoded) {
    return (
      <Button
        variant="outline"
        className={`flex items-center gap-2 ${isMobile ? commonClasses : 'hidden md:flex'}`}
        onClick={() => navigate('/profile')}
      >
        <div className="w-7 h-7 bg-indigo-500 text-white rounded-full flex items-center justify-center text-xs font-bold">
          {decoded.unique_name.charAt(0).toUpperCase()}
        </div>
        <span>{decoded.unique_name}</span>
      </Button>
    );
  }

  return (
    <Button
      variant="default"
      className={`bg-indigo-600 hover:bg-indigo-700 ${isMobile ? commonClasses : 'hidden md:flex'}`}
      onClick={() => navigate('/login')}
    >
      <User className="h-4 w-4 mr-2" />
      <span>Đăng Nhập</span>
    </Button>
  );
};

// =================================================================
// Main Header Component
// =================================================================
const Header: React.FC = () => {
  // --- HOOKS ---
  const navigate = useNavigate();
  const token = localStorage.getItem('accessToken');

  // --- STATE MANAGEMENT ---
  const [isMenuOpen, setIsMenuOpen] = useState<boolean>(false);
  const [searchQuery, setSearchQuery] = useState<string>("");

  // --- PERFORMANCE OPTIMIZATION ---
  // Memoize the decoded token to prevent re-calculating on every render.
  // It will only re-decode when the token itself changes.
  const decodedToken = useMemo(() => {
    try {
      return token ? jwtDecode<DecodedToken>(token) : null;
    } catch (error) {
      console.error("Invalid token:", error);
      return null;
    }
  }, [token]);

  // --- SIDE EFFECTS ---
  // Effect to close the mobile menu if the window is resized to desktop width.
  useEffect(() => {
    const handleResize = () => {
      if (window.innerWidth >= 768) { // md breakpoint
        setIsMenuOpen(false);
      }
    };
    window.addEventListener('resize', handleResize);
    // Cleanup listener on component unmount
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  // --- CONFIGURATION ---
  const menuItems = [
    { label: "Trang Chủ", href: "/" },
    { label: "Phim", href: "/movies" },
    { label: "Rạp", href: "/cinemas" },
    { label: "Liên Hệ", href: "/contact" },
  ];

  // --- RENDER ---
  return (
    <header className="bg-white/80 backdrop-blur-lg shadow-sm sticky top-0 z-50 border-b border-gray-200">
      <div className="container mx-auto px-4">
        <div className="flex items-center justify-between h-16">
          {/* Logo Section */}
          <NavLink to="/" className="flex items-center gap-2 cursor-pointer">
            <div className="w-10 h-10 bg-gradient-to-br from-indigo-500 to-purple-600 rounded-lg flex items-center justify-center shadow-md">
              <Film className="text-white" />
            </div>
            <span className="text-2xl font-bold text-gray-800 hidden sm:block">CinemaHub</span>
          </NavLink>

          {/* Desktop Navigation */}
          <nav className="hidden md:flex items-center gap-8">
            {menuItems.map((item) => (
              // Use NavLink for SPA routing and active link styling.
              <NavLink
                key={item.label}
                to={item.href}
                className={({ isActive }) =>
                  `text-gray-600 hover:text-indigo-600 transition-colors duration-200 font-medium pb-1 ${isActive ? 'text-indigo-600 border-b-2 border-indigo-500' : 'border-b-2 border-transparent'}`
                }
              >
                {item.label}
              </NavLink>
            ))}
          </nav>

          {/* Search and Auth Section */}
          <div className="flex items-center gap-4">
            <div className="hidden md:flex items-center relative">
              <Input
                type="text"
                placeholder="Tìm kiếm phim..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="w-56 pr-10"
              />
              <Search className="absolute right-3 h-4 w-4 text-gray-400" />
            </div>

            <AuthButtons decoded={decodedToken} />

            {/* Mobile Menu Toggle */}
            <Button
              variant="ghost"
              size="icon"
              className="md:hidden"
              onClick={() => setIsMenuOpen(!isMenuOpen)}
            >
              {isMenuOpen ? <X className="h-6 w-6" /> : <Menu className="h-6 w-6" />}
            </Button>
          </div>
        </div>

        {/* Mobile Menu with smooth transition */}
        <div className={`md:hidden overflow-hidden transition-all duration-300 ease-in-out ${isMenuOpen ? 'max-h-96 py-4 border-t' : 'max-h-0'}`}>
          <div className="flex flex-col gap-4">
            {menuItems.map((item) => (
              <NavLink
                key={`mobile-${item.label}`}
                to={item.href}
                className="text-gray-700 hover:text-indigo-600 font-medium py-2 text-center"
                onClick={() => setIsMenuOpen(false)} // Close menu on navigation
              >
                {item.label}
              </NavLink>
            ))}
            <div className="border-t pt-4 mt-2">
              <AuthButtons decoded={decodedToken} isMobile={true} />
            </div>
          </div>
        </div>
      </div>
    </header>
  );
};

export default Header;