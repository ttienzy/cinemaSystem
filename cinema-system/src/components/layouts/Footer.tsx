import React from 'react';
import { Link } from 'react-router-dom'; // Using Link for SPA navigation
import { Facebook, Instagram, Twitter, Youtube, Mail, Phone, MapPin, Film, ArrowRight } from "lucide-react";
import { Button } from "../ui/Button";
import { Input } from "../ui/Input";

// =================================================================
// DATA (defined outside the component to prevent re-creation on renders)
// =================================================================
const quickLinks = [
  { label: "Trang Chủ", href: "/" },
  { label: "Phim Đang Chiếu", href: "/movies" },
  { label: "Hệ Thống Rạp", href: "/cinemas" },
  { label: "Khuyến Mãi", href: "/promotions" },
];

const supportLinks = [
  { label: "Câu Hỏi Thường Gặp", href: "/faq" },
  { label: "Chính Sách Bảo Mật", href: "/privacy" },
  { label: "Điều Khoản Sử Dụng", href: "/terms" },
  { label: "Liên Hệ", href: "/contact" },
];

const socialLinks = [
  { icon: Facebook, href: "#", label: "Facebook" },
  { icon: Instagram, href: "#", label: "Instagram" },
  { icon: Twitter, href: "#", label: "Twitter" },
  { icon: Youtube, href: "#", label: "Youtube" },
];

// =================================================================
// Reusable Child Component for Footer Link Columns
// This makes the main component cleaner and easier to read (DRY principle).
// =================================================================
interface FooterLinkColumnProps {
  title: string;
  links: { label: string; href: string }[];
}

const FooterLinkColumn: React.FC<FooterLinkColumnProps> = ({ title, links }) => (
  <div>
    <h4 className="text-lg font-semibold text-white mb-6">{title}</h4>
    <ul className="space-y-3">
      {links.map((link) => (
        <li key={link.label}>
          <Link
            to={link.href}
            className="text-gray-400 hover:text-indigo-400 hover:translate-x-1 transition-all duration-200 flex items-center group"
          >
            <ArrowRight className="w-3 h-3 mr-3 opacity-0 group-hover:opacity-100 transition-opacity duration-200" />
            {link.label}
          </Link>
        </li>
      ))}
    </ul>
  </div>
);

// =================================================================
// Main Footer Component
// =================================================================
const Footer: React.FC = () => {
  const currentYear = new Date().getFullYear();

  return (
    <footer className="bg-gray-900 text-white border-t-4 border-indigo-500">
      {/* Section 1: Newsletter Signup (more engaging than app downloads) */}
      <div className="py-12 bg-gray-800/50">
        <div className="container mx-auto px-4 text-center">
          <h3 className="text-2xl font-bold text-white mb-2">Đừng Bỏ Lỡ Phim Hay!</h3>
          <p className="text-gray-400 mb-6 max-w-2xl mx-auto">Đăng ký để nhận thông tin về phim mới, lịch chiếu và các chương trình khuyến mãi đặc biệt.</p>
          <form className="flex flex-col sm:flex-row gap-3 max-w-md mx-auto">
            <Input
              type="email"
              placeholder="Nhập email của bạn"
              className="bg-gray-700 border-gray-600 text-white placeholder-gray-500 flex-grow"
              aria-label="Email for newsletter"
            />
            <Button type="submit" className="bg-indigo-600 hover:bg-indigo-700">
              Đăng Ký <Mail className="w-4 h-4 ml-2" />
            </Button>
          </form>
        </div>
      </div>

      {/* Section 2: Main Footer Content */}
      <div className="py-16">
        <div className="container mx-auto px-4">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-10">
            {/* Column 1: Company Info */}
            <div className="space-y-6">
              <Link to="/" className="flex items-center gap-2">
                <div className="w-10 h-10 bg-gradient-to-br from-indigo-500 to-purple-600 rounded-lg flex items-center justify-center">
                  <Film className="text-white" />
                </div>
                <span className="text-2xl font-bold text-white">CinemaHub</span>
              </Link>
              <p className="text-gray-400 leading-relaxed">
                Trải nghiệm điện ảnh đỉnh cao với công nghệ hiện đại và dịch vụ tận tâm.
              </p>
              <div className="flex space-x-3">
                {socialLinks.map((social) => (
                  <a
                    key={social.label}
                    href={social.href}
                    className="w-9 h-9 bg-gray-800 rounded-full flex items-center justify-center text-gray-400 hover:bg-indigo-500 hover:text-white transition-all duration-200"
                    title={social.label}
                    aria-label={social.label}
                  >
                    <social.icon size={18} />
                  </a>
                ))}
              </div>
            </div>

            {/* Column 2 & 3: Link Columns (using the reusable component) */}
            <FooterLinkColumn title="Liên Kết Nhanh" links={quickLinks} />
            <FooterLinkColumn title="Hỗ Trợ & Chính Sách" links={supportLinks} />

            {/* Column 4: Contact Info */}
            <div>
              <h4 className="text-lg font-semibold text-white mb-6">Thông Tin Liên Hệ</h4>
              <div className="space-y-4 text-gray-400">
                <div className="flex items-start gap-3"><MapPin size={16} className="text-indigo-400 mt-1" /><span>123 Nguyễn Huệ, Quận 1, TP.HCM</span></div>
                <div className="flex items-start gap-3"><Phone size={16} className="text-indigo-400" /><span>Hotline: 1900 6017</span></div>
                <div className="flex items-start gap-3"><Mail size={16} className="text-indigo-400" /><span>support@cinemahub.vn</span></div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Section 3: Bottom Bar */}
      <div className="border-t border-gray-800 py-6">
        <div className="container mx-auto px-4 text-center text-gray-500 text-sm">
          <p>© {currentYear} CinemaHub. All Rights Reserved. Designed with passion.</p>
        </div>
      </div>
    </footer>
  );
};

export default Footer;