using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace QuanLyNhaThuoc
{
	public static class QuanLyKhachHang
	{
		public static List<KhachHang> DanhSachKhachHang = new List<KhachHang>();
		public static List<LichSuMuaHang> DanhSachTatCaDonHang = new List<LichSuMuaHang>();

		private static string fileName = "khachhang.txt";


		// ================================================================
		// KHỞI TẠO
		// ================================================================
		static QuanLyKhachHang()
		{
			DocTuFile();
		}


		// ================================================================
		// XÁC ĐỊNH CẤP VIP THEO ĐIỂM BAN ĐẦU
		//
		// 1 = Thành Viên
		// 2 = VIP Bạc       >= 500
		// 3 = VIP Vàng      >= 2000
		// 4 = Kim Cương     >= 5000
		// ================================================================
		private static int LayCapVipTheoDiem(int diem)
		{
			if (diem >= 5000)
				return 4;

			if (diem >= 2000)
				return 3;

			if (diem >= 500)
				return 2;

			return 1;
		}


		// ================================================================
		// ĐỌC FILE KHÁCH HÀNG
		//
		// Hỗ trợ cả FILE CŨ:
		// SĐT|Tên|Điểm
		//
		// Và FILE MỚI:
		// SĐT|Tên|ĐiểmKhảDụng|CapVip|NgayBatDauHang
		//
		// Ví dụ:
		// 0963555666|Bùi Thanh Lan|5000|4|2026-08-23 16:50:00
		// ================================================================
		public static void DocTuFile()
		{
			DanhSachKhachHang.Clear();

			if (!File.Exists(fileName))
			{
				TaoFileMau();
			}

			bool canNangCapFileCu = false;

			try
			{
				string[] lines = File.ReadAllLines(
					fileName,
					Encoding.UTF8
				);

				foreach (string line in lines)
				{
					if (string.IsNullOrWhiteSpace(line))
						continue;

					string[] parts = line.Split('|');

					if (parts.Length < 3)
						continue;


					// =====================================================
					// 1. THÔNG TIN CƠ BẢN
					// =====================================================
					string sdt = parts[0].Trim();
					string hoTen = parts[1].Trim();

					int diem = int.TryParse(
						parts[2].Trim(),
						out int pts
					)
						? pts
						: 0;


					// =====================================================
					// 2. ĐỌC CẤP VIP
					// =====================================================
					int capVip;

					if (parts.Length >= 4 &&
						int.TryParse(
							parts[3].Trim(),
							out int cap))
					{
						capVip = cap;
					}
					else
					{
						// File cũ chưa có CapVip
						capVip = LayCapVipTheoDiem(diem);

						canNangCapFileCu = true;
					}


					// Bảo vệ dữ liệu
					if (capVip < 1)
						capVip = 1;

					if (capVip > 4)
						capVip = 4;


					// =====================================================
					// 3. ĐỌC NGÀY BẮT ĐẦU HẠNG
					// =====================================================
					DateTime ngayBatDauVip;

					if (parts.Length >= 5 &&
						DateTime.TryParse(
							parts[4].Trim(),
							out DateTime ngayVip))
					{
						ngayBatDauVip = ngayVip;
					}
					else
					{
						/*
						 * FILE CŨ chưa có ngày bắt đầu VIP.
						 *
						 * Lần đầu tiên lấy ngày hiện tại.
						 *
						 * Sau đó file sẽ được tự động chuyển sang
						 * định dạng mới 5 cột nên các lần sau
						 * KHÔNG reset ngày nữa.
						 */
						ngayBatDauVip = DateTime.Now;

						canNangCapFileCu = true;
					}


					// =====================================================
					// 4. TẠO KHÁCH HÀNG
					// =====================================================
					KhachHang kh = new KhachHang
					{
						SoDienThoai = sdt,
						HoTen = hoTen,

						/*
						 * DiemTichLuy setter sẽ tạo các BatchDiem
						 * tương ứng với số điểm đang có.
						 */
						DiemTichLuy = diem,

						/*
						 * Sau đó ghi đè lại đúng thông tin VIP
						 * đã lưu trong file.
						 */
						CapVip = capVip,
						NgayThangHang = ngayBatDauVip
					};


					DanhSachKhachHang.Add(kh);
				}


				// =====================================================
				// FILE CŨ 3 CỘT
				// -> TỰ ĐỘNG CHUYỂN THÀNH FILE MỚI 5 CỘT
				// =====================================================
				if (canNangCapFileCu)
				{
					LuuVaoFile();
				}
			}
			catch (Exception ex)
			{
				System.Windows.Forms.MessageBox.Show(
					"Lỗi đọc file khách hàng: " + ex.Message
				);
			}
		}


		// ================================================================
		// LƯU FILE
		//
		// QUAN TRỌNG:
		// Phải lưu cả CapVip + NgayThangHang.
		//
		// Nếu chỉ lưu 3 cột thì đóng app xong mở lại
		// sẽ mất ngày bảo lưu VIP.
		// ================================================================
		public static void LuuVaoFile()
		{
			try
			{
				List<string> lines = new List<string>();

				foreach (var kh in DanhSachKhachHang)
				{
					if (kh == null)
						continue;

					string line =
						$"{kh.SoDienThoai}|" +
						$"{kh.HoTen}|" +
						$"{kh.DiemKhaDung}|" +
						$"{kh.CapVip}|" +
						$"{kh.NgayThangHang:yyyy-MM-dd}";
					lines.Add(line);
				}

				File.WriteAllLines(
					fileName,
					lines,
					Encoding.UTF8
				);
			}
			catch (Exception ex)
			{
				System.Windows.Forms.MessageBox.Show(
					"Lỗi ghi file khách hàng: " + ex.Message
				);
			}
		}


		// ================================================================
		// TẠO FILE MẪU
		//
		// Vẫn giữ dữ liệu mẫu cũ.
		// Khi chương trình đọc lần đầu sẽ tự chuyển sang 5 cột.
		// ================================================================
		private static void TaoFileMau()
		{
			string defaultData =
@"0912345678|Nguyễn Văn An|150
0987654321|Trần Thị Bình|80
0905123456|Lê Hoàng Cường|210
0935999888|Phạm Minh Đức|3000
0978111222|Vũ Thị Hoa|320
0914333444|Đặng Quốc Khánh|1500
0963555666|Bùi Thanh Lan|5000
0908777888|Đỗ Quang Nam|430
0942112233|Nông Văn Phúc|900
0989000111|Hoàng Anh Tú|180";

			File.WriteAllText(
				fileName,
				defaultData,
				Encoding.UTF8
			);
		}


		// ================================================================
		// TÌM KIẾM
		// ================================================================
		public static List<KhachHang> TimKiem(string sdt)
		{
			if (string.IsNullOrWhiteSpace(sdt))
			{
				return DanhSachKhachHang.ToList();
			}

			return DanhSachKhachHang
				.Where(k =>
					k != null &&
					k.SoDienThoai != null &&
					k.SoDienThoai.Contains(sdt))
				.ToList();
		}


		public static KhachHang TimChinhXac(string sdt)
		{
			return DanhSachKhachHang
				.FirstOrDefault(
					k =>
						k != null &&
						k.SoDienThoai == sdt
				);
		}


		// ================================================================
		// THÊM KHÁCH HÀNG
		// ================================================================
		public static bool ThemKhachHang(
			string sdt,
			string hoTen)
		{
			if (TimChinhXac(sdt) != null)
				return false;


			KhachHang khMoi = new KhachHang
			{
				SoDienThoai = sdt,
				HoTen = hoTen,

				DiemTichLuy = 0,

				// Khách mới bắt đầu là Thành Viên
				CapVip = 1,

				// Chu kỳ thành viên bắt đầu từ ngày đăng ký
				NgayThangHang = DateTime.Now
			};


			DanhSachKhachHang.Add(khMoi);

			LuuVaoFile();

			return true;
		}


		// ================================================================
		// CẬP NHẬT SỐ ĐIỆN THOẠI
		// ================================================================
		public static bool CapNhatSoDienThoai(
			string sdtCu,
			string sdtMoi)
		{
			var kh = TimChinhXac(sdtCu);

			if (kh == null)
				return false;


			if (sdtCu != sdtMoi &&
				TimChinhXac(sdtMoi) != null)
			{
				return false;
			}


			kh.SoDienThoai = sdtMoi;


			// Cập nhật SĐT trong lịch sử riêng của khách
			if (kh.DanhSachLichSu != null)
			{
				foreach (var dh in kh.DanhSachLichSu)
				{
					if (dh != null)
					{
						dh.SoDienThoai = sdtMoi;
					}
				}
			}


			// Cập nhật cả danh sách đơn hàng chung của class này
			if (DanhSachTatCaDonHang != null)
			{
				foreach (var dh in DanhSachTatCaDonHang
					.Where(d =>
						d != null &&
						d.SoDienThoai == sdtCu))
				{
					dh.SoDienThoai = sdtMoi;
				}
			}


			LuuVaoFile();

			return true;
		}


		// ================================================================
		// XÓA KHÁCH HÀNG
		// ================================================================
		public static bool XoaKhachHang(string sdt)
		{
			var kh = TimChinhXac(sdt);

			if (kh == null)
				return false;


			DanhSachKhachHang.Remove(kh);

			LuuVaoFile();

			return true;
		}


		// ================================================================
		// CỘNG ĐIỂM VÀ TẠO ĐƠN
		//
		// 1.000 VNĐ = 1 điểm
		//
		// QUAN TRỌNG:
		// Không dùng:
		//
		// kh.DiemTichLuy += diemCong;
		//
		// nữa.
		//
		// Vì DiemTichLuy không còn là ví điểm trực tiếp.
		// Phải dùng kh.CongDiem().
		// ================================================================
		public static void CongDiemVaTaoDon(
			string sdt,
			string maHang,
			decimal soTien,
			string ghiChu)
		{
			var kh = TimChinhXac(sdt);

			if (kh == null)
				return;


			if (soTien < 0)
				soTien = 0;


			int diemCong =
				(int)(soTien / 1000);


			if (string.IsNullOrWhiteSpace(maHang))
			{
				maHang = "THUOC-GENERAL";
			}

			if (string.IsNullOrWhiteSpace(ghiChu))
			{
				ghiChu = "Mua thuốc tích điểm";
			}


			// =====================================================
			// 1. TẠO ĐƠN HÀNG TRƯỚC
			// =====================================================
			var donHang = new LichSuMuaHang
			{
				MaHoaDon =
					"HD" +
					(DanhSachTatCaDonHang.Count + 1)
					.ToString("D3"),

				NgayMua = DateTime.Now,

				SoDienThoai = sdt,

				MaHangHoa = maHang.Trim(),

				ThanhTien = soTien,

				DiemCong = diemCong,

				DiemTru = 0,

				GhiChu = ghiChu.Trim()
			};


			// =====================================================
			// 2. THÊM VÀO LỊCH SỬ KHÁCH HÀNG
			//
			// Phải thêm lịch sử TRƯỚC CongDiem()
			// để hệ thống VIP nhìn thấy điểm của đơn mới.
			// =====================================================
			if (kh.DanhSachLichSu == null)
			{
				kh.DanhSachLichSu =
					new List<LichSuMuaHang>();
			}

			kh.DanhSachLichSu.Add(donHang);


			// =====================================================
			// 3. THÊM VÀO DANH SÁCH ĐƠN HÀNG CHUNG
			// =====================================================
			DanhSachTatCaDonHang.Add(donHang);


			// =====================================================
			// 4. CỘNG ĐIỂM BẰNG HỆ THỐNG BATCH
			// =====================================================
			if (diemCong > 0)
			{
				kh.CongDiem(diemCong);
			}


			// =====================================================
			// 5. KIỂM TRA VIP
			//
			// CongDiem() đã kiểm tra,
			// gọi lại vẫn an toàn và giúp rõ luồng xử lý.
			// =====================================================
			kh.KiemTraCapNhatHangVipLongChau();


			// =====================================================
			// 6. LƯU
			// =====================================================
			LuuVaoFile();
		}


		// ================================================================
		// TRỪ ĐIỂM
		//
		// Chỉ trừ ĐIỂM KHẢ DỤNG.
		//
		// KHÔNG hạ VIP.
		// KHÔNG đổi ngày giữ VIP.
		// ================================================================
		public static bool TruDiem(
			string sdt,
			int diemTru)
		{
			var kh = TimChinhXac(sdt);


			if (kh == null)
				return false;

			if (diemTru <= 0)
				return false;


			// Không đủ điểm khả dụng
			if (kh.DiemKhaDung < diemTru)
				return false;


			// Trừ FIFO từ lô sắp hết hạn trước
			kh.TruDiem(diemTru);


			/*
			 * Tuyệt đối KHÔNG làm:
			 *
			 * kh.DiemTichLuy -= diemTru;
			 *
			 * vì sẽ làm sai hệ thống VIP.
			 */


			LuuVaoFile();

			return true;
		}


		// ================================================================
		// RESET ĐIỂM KHẢ DỤNG
		//
		// Chỉ reset ví điểm.
		//
		// KHÔNG reset:
		// - CapVip
		// - NgayThangHang
		//
		// vì đổi/tiêu/reset điểm không được làm mất hạng đang bảo lưu.
		// ================================================================
		public static void ResetDiem(string sdt)
		{
			var kh = TimChinhXac(sdt);

			if (kh == null)
				return;


			if (kh.DanhSachDiem == null)
			{
				kh.DanhSachDiem =
					new List<BatchDiem>();
			}
			else
			{
				kh.DanhSachDiem.Clear();
			}


			/*
			 * Tổng điểm runtime về 0.
			 *
			 * Không thay CapVip.
			 * Không thay NgayThangHang.
			 */
			kh.TongDiemTichLuy = 0;


			LuuVaoFile();
		}


		// ================================================================
		// LỌC LỊCH SỬ
		// ================================================================
		public static List<LichSuMuaHang> LocLichSu(
			string sdt,
			DateTime? ngay = null)
		{
			var kh = TimChinhXac(sdt);

			if (kh == null ||
				kh.DanhSachLichSu == null)
			{
				return new List<LichSuMuaHang>();
			}


			if (ngay.HasValue)
			{
				return kh.DanhSachLichSu
					.Where(
						d =>
							d != null &&
							d.NgayMua.Date ==
							ngay.Value.Date
					)
					.ToList();
			}


			return kh.DanhSachLichSu;
		}


		// ================================================================
		// DOANH THU HÔM NAY
		// ================================================================
		public static List<LichSuMuaHang> LayDoanhThuHomNay(
			out decimal tongTien,
			out int tongDon)
		{
			var todayOrders =
				DanhSachTatCaDonHang
				.Where(
					o =>
						o != null &&
						o.NgayMua.Date ==
						DateTime.Today
				)
				.ToList();


			tongTien =
				todayOrders.Sum(
					o => o.ThanhTien
				);


			tongDon =
				todayOrders.Count;


			return todayOrders;
		}
	}
}