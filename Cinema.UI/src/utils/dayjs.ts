import dayjs from 'dayjs';
import utc from 'dayjs/plugin/utc';
import timezone from 'dayjs/plugin/timezone';
import customParseFormat from 'dayjs/plugin/customParseFormat';
import 'dayjs/locale/vi'; // Import Vietnamese locale if needed

dayjs.extend(utc);
dayjs.extend(timezone);
dayjs.extend(customParseFormat);

// Set default timezone for the cinema system (Vietnam)
dayjs.tz.setDefault('Asia/Ho_Chi_Minh');
dayjs.locale('vi');

export default dayjs;
