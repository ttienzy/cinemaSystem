import { useState } from "react";
import { Search, User, Menu, X } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { Button } from "../ui/Button";
import { Input } from "../ui/Input";
import { jwtDecode } from "jwt-decode";
import type { DecodedToken } from "../../types/auth.types";


const Header = () => {

  const token = localStorage.getItem('accessToken');
  const decoded = token ? jwtDecode<DecodedToken>(token) : null;

  const navigate = useNavigate();
  const [isMenuOpen, setIsMenuOpen] = useState<boolean>(false);
  const [searchQuery, setSearchQuery] = useState<string>("");

  const menuItems = [
    { label: "Trang Chủ", href: "#" },
    { label: "Phim", href: "/movies" },
    { label: "Rạp", href: "/cinemas" },
    { label: "Liên Hệ", href: "/contact" },
  ];

  return (
    <header className="bg-white shadow-md sticky top-0 z-50">
      <div className="container mx-auto px-4">
        <div className="flex items-center justify-between h-16">
          {/* Logo */}
          <div className="flex items-center space-x-2">
            <div className="w-10 h-10 bg-red-600 rounded-lg flex items-center justify-center">
              <span className="text-white font-bold text-xl">C</span>
            </div>
            <span className="text-2xl font-bold text-gray-800">CinemaHub</span>
          </div>

          {/* Desktop Navigation */}
          <nav className="hidden md:flex items-center space-x-8">
            {menuItems.map((item) => (
              <a
                key={item.label}
                href={item.href}
                className="text-gray-600 hover:text-red-600 transition-colors duration-200 font-medium"
              >
                {item.label}
              </a>
            ))}
          </nav>

          {/* Search and Login */}
          <div className="flex items-center space-x-4">
            {/* Search */}
            <div className="hidden md:flex items-center relative">
              <Input
                type="text"
                placeholder="Tìm phim, rạp..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="w-64 pr-10"
              />
              <Search className="absolute right-3 h-4 w-4 text-gray-400" />
            </div>

            {/* Login Button */}
            {decoded ? (
              <Button
                variant="outline"
                className="hidden md:flex items-center space-x-2 cursor-pointer"
                onClick={() => navigate('/profile')}
              >
                <div className="w-6 h-6 bg-blue-500 text-white rounded-full flex items-center justify-center text-xs font-medium">
                  {decoded.unique_name.charAt(0).toUpperCase()}
                </div>
                <span>{decoded.unique_name}</span>
              </Button>
            ) : (
              <Button
                variant="outline"
                className="hidden md:flex items-center space-x-2 cursor-pointer"
                onClick={() => navigate('/login')}
              >
                <User className="h-4 w-4" />
                <span>Đăng Nhập</span>
              </Button>
            )}

            {/* Mobile Menu Button */}
            <Button
              variant="ghost"
              size="sm"
              className="md:hidden"
              onClick={() => setIsMenuOpen(!isMenuOpen)}
            >
              {isMenuOpen ? (
                <X className="h-6 w-6" />
              ) : (
                <Menu className="h-6 w-6" />
              )}
            </Button>
          </div>
        </div>

        {/* Mobile Menu */}
        {isMenuOpen && (
          <div className="md:hidden py-4 border-t">
            <div className="flex flex-col space-y-4">
              {/* Mobile Search */}
              <div className="relative">
                <Input
                  type="text"
                  placeholder="Tìm phim, rạp..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  className="w-full pr-10"
                />
                <Search className="absolute right-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
              </div>

              {/* Mobile Navigation */}
              {menuItems.map((item) => (
                <a
                  key={item.label}
                  href={item.href}
                  className="text-gray-600 hover:text-red-600 transition-colors duration-200 font-medium py-2"
                  onClick={() => setIsMenuOpen(false)}
                >
                  {item.label}
                </a>
              ))}

              {/* Mobile Login */}
              {decoded ? (
                <Button
                  variant="outline"
                  className="hidden md:flex items-center space-x-2 cursor-pointer"
                  onClick={() => navigate('/profile')}
                >
                  <div className="w-6 h-6 bg-blue-500 text-white rounded-full flex items-center justify-center text-xs font-medium">
                    {decoded.unique_name.charAt(0).toUpperCase()}
                  </div>
                  <span>{decoded.unique_name}</span>
                </Button>
              ) : (
                <Button
                  variant="outline"
                  className="hidden md:flex items-center space-x-2 cursor-pointer"
                  onClick={() => navigate('/login')}
                >
                  <User className="h-4 w-4" />
                  <span>Đăng Nhập</span>
                </Button>
              )}
            </div>
          </div>
        )}
      </div>
    </header>
  );
};

export default Header;
