import {
  Facebook,
  Instagram,
  Twitter,
  Youtube,
  Mail,
  Phone,
  MapPin,
  Clock,
} from "lucide-react";
import { Button } from "../ui/Button";
import { Input } from "../ui/Input";

const Footer = () => {
  const currentYear = new Date().getFullYear();

  const quickLinks = [
    { label: "Trang Chủ", href: "#" },
    { label: "Phim Đang Chiếu", href: "#movies" },
    { label: "Phim Sắp Chiếu", href: "#coming-soon" },
    { label: "Hệ Thống Rạp", href: "#cinemas" },
    { label: "Khuyến Mãi", href: "#promotions" },
    { label: "Tin Tức", href: "#news" },
  ];

  const supportLinks = [
    { label: "Hướng Dẫn Đặt Vé", href: "#guide" },
    { label: "Câu Hỏi Thường Gặp", href: "#faq" },
    { label: "Chính Sách Bảo Mật", href: "#privacy" },
    { label: "Điều Khoản Sử Dụng", href: "#terms" },
    { label: "Chính Sách Hoàn Tiền", href: "#refund" },
    { label: "Liên Hệ Hỗ Trợ", href: "#support" },
  ];

  const socialLinks = [
    { icon: Facebook, href: "#", label: "Facebook" },
    { icon: Instagram, href: "#", label: "Instagram" },
    { icon: Twitter, href: "#", label: "Twitter" },
    { icon: Youtube, href: "#", label: "Youtube" },
  ];

  return (
    <footer className="bg-gray-900 text-white">


      {/* Main Footer Content */}
      <div className="py-12">
        <div className="container mx-auto px-4">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
            {/* Company Info */}
            <div>
              <div className="flex items-center space-x-2 mb-6">
                <div className="w-10 h-10 bg-red-600 rounded-lg flex items-center justify-center">
                  <span className="text-white font-bold text-xl">C</span>
                </div>
                <span className="text-2xl font-bold">CinemaHub</span>
              </div>
              <p className="text-gray-400 mb-6 leading-relaxed">
                Hệ thống rạp chiếu phim hàng đầu Việt Nam với công nghệ hiện đại
                và dịch vụ tốt nhất. Mang đến trải nghiệm giải trí đỉnh cao cho
                mọi khách hàng.
              </p>

              {/* Contact Info */}
              <div className="space-y-3">
                <div className="flex items-center space-x-3 text-gray-400">
                  <Phone className="h-4 w-4 text-red-600" />
                  <span>Hotline: 1900 6017</span>
                </div>
                <div className="flex items-center space-x-3 text-gray-400">
                  <Mail className="h-4 w-4 text-red-600" />
                  <span>info@cinemahub.vn</span>
                </div>
                <div className="flex items-start space-x-3 text-gray-400">
                  <MapPin className="h-4 w-4 text-red-600 mt-1" />
                  <span>123 Nguyễn Huệ, Quận 1, TP.HCM</span>
                </div>
                <div className="flex items-center space-x-3 text-gray-400">
                  <Clock className="h-4 w-4 text-red-600" />
                  <span>Hỗ trợ 24/7</span>
                </div>
              </div>
            </div>

            {/* Quick Links */}
            <div>
              <h4 className="text-lg font-semibold mb-6">Liên Kết Nhanh</h4>
              <ul className="space-y-3">
                {quickLinks.map((link) => (
                  <li key={link.label}>
                    <a
                      href={link.href}
                      className="text-gray-400 hover:text-white transition-colors duration-200 flex items-center group"
                    >
                      <span className="w-2 h-2 bg-red-600 rounded-full mr-3 opacity-0 group-hover:opacity-100 transition-opacity duration-200"></span>
                      {link.label}
                    </a>
                  </li>
                ))}
              </ul>
            </div>

            {/* Support */}
            <div>
              <h4 className="text-lg font-semibold mb-6">Hỗ Trợ</h4>
              <ul className="space-y-3">
                {supportLinks.map((link) => (
                  <li key={link.label}>
                    <a
                      href={link.href}
                      className="text-gray-400 hover:text-white transition-colors duration-200 flex items-center group"
                    >
                      <span className="w-2 h-2 bg-red-600 rounded-full mr-3 opacity-0 group-hover:opacity-100 transition-opacity duration-200"></span>
                      {link.label}
                    </a>
                  </li>
                ))}
              </ul>
            </div>

            {/* Social & App Download */}
            <div>
              <h4 className="text-lg font-semibold mb-6">
                Kết Nối Với Chúng Tôi
              </h4>

              {/* Social Links */}
              <div className="flex space-x-4 mb-6">
                {socialLinks.map((social) => (
                  <a
                    key={social.label}
                    href={social.href}
                    className="w-10 h-10 bg-gray-800 rounded-lg flex items-center justify-center hover:bg-red-600 transition-colors duration-200 group"
                    title={social.label}
                  >
                    <social.icon className="h-5 w-5 text-gray-400 group-hover:text-white" />
                  </a>
                ))}
              </div>

              {/* App Download */}
              <div>
                <h5 className="font-medium mb-4">Tải Ứng Dụng</h5>
                <div className="space-y-3">
                  <a href="#" className="block">
                    <div className="bg-gray-800 rounded-lg p-3 hover:bg-gray-700 transition-colors duration-200">
                      <div className="flex items-center space-x-3">
                        <div className="w-8 h-8 bg-white rounded flex items-center justify-center">
                          <span className="text-black font-bold text-xs">
                            iOS
                          </span>
                        </div>
                        <div>
                          <div className="text-xs text-gray-400">Tải trên</div>
                          <div className="text-sm font-medium">App Store</div>
                        </div>
                      </div>
                    </div>
                  </a>
                  <a href="#" className="block">
                    <div className="bg-gray-800 rounded-lg p-3 hover:bg-gray-700 transition-colors duration-200">
                      <div className="flex items-center space-x-3">
                        <div className="w-8 h-8 bg-green-500 rounded flex items-center justify-center">
                          <span className="text-white font-bold text-xs">
                            GP
                          </span>
                        </div>
                        <div>
                          <div className="text-xs text-gray-400">Tải trên</div>
                          <div className="text-sm font-medium">Google Play</div>
                        </div>
                      </div>
                    </div>
                  </a>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Bottom Bar */}
      <div className="border-t border-gray-800 py-6">
        <div className="container mx-auto px-4">
          <div className="flex flex-col md:flex-row items-center justify-between">
            <div className="text-gray-400 text-sm mb-4 md:mb-0">
              © {currentYear} CinemaHub. Tất cả quyền được bảo lưu.
            </div>
            <div className="flex flex-wrap items-center space-x-6 text-sm text-gray-400">
              <a
                href="#"
                className="hover:text-white transition-colors duration-200"
              >
                Chính Sách Bảo Mật
              </a>
              <a
                href="#"
                className="hover:text-white transition-colors duration-200"
              >
                Điều Khoản Sử Dụng
              </a>
              <a
                href="#"
                className="hover:text-white transition-colors duration-200"
              >
                Cookies
              </a>
              <a
                href="#"
                className="hover:text-white transition-colors duration-200"
              >
                Sitemap
              </a>
            </div>
          </div>
        </div>
      </div>
    </footer>
  );
};

export default Footer;
