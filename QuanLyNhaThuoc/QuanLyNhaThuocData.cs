using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace QuanLyNhaThuoc
{
	public static class QuanLyNhaThuocData
	{
		public static List<Thuoc> DanhSachThuoc = new List<Thuoc>();
		public static List<KhachHang> DanhSachKhachHang = new List<KhachHang>();
		public static List<LichSuMuaHang> DanhSachDonHang = new List<LichSuMuaHang>();

		private static string fileThuoc = "thuoc.txt";
		private static string fileKhachHang = "khachhang.txt";
		private static string fileDonHang = "donhang.txt";


		// ================================================================
		// KHỞI TẠO DATA
		// ================================================================
		static QuanLyNhaThuocData()
		{
			DocTatCaData();
		}


		public static void DocTatCaData()
		{
			DocFileThuoc();
			DocFileKhachHang();
			DocFileDonHang();
		}


		// ================================================================
		// XỬ LÝ FILE THUỐC
		// GIỮ NGUYÊN CHỨC NĂNG CŨ
		// ================================================================
		private static void DocFileThuoc()
		{
			DanhSachThuoc.Clear();

			if (!File.Exists(fileThuoc))
			{
				TaoFileThuocMau();
			}

			try
			{
				string[] lines = File.ReadAllLines(
					fileThuoc,
					Encoding.UTF8
				);

				foreach (var line in lines)
				{
					if (string.IsNullOrWhiteSpace(line))
						continue;

					string[] p = line.Split('|');


					// Cấu trúc thuốc gồm 11 trường dữ liệu
					if (p.Length >= 11)
					{
						DanhSachThuoc.Add(
							new Thuoc
							{
								MaThuoc = p[0].Trim(),

								TenThuoc = p[1].Trim(),

								ThanhPhan = p[2].Trim(),

								CoSoSanXuat = p[3].Trim(),

								DonViTinh = p[4].Trim(),

								GiaHop =
									decimal.TryParse(
										p[5].Trim(),
										out decimal gh
									)
										? gh
										: 0,

								GiaVi =
									decimal.TryParse(
										p[6].Trim(),
										out decimal gv
									)
										? gv
										: 0,

								GiaVien =
									decimal.TryParse(
										p[7].Trim(),
										out decimal gvn
									)
										? gvn
										: 0,

								SoViTrongHop =
									int.TryParse(
										p[8].Trim(),
										out int sv
									)
										? sv
										: 10,

								SoVienTrongVi =
									int.TryParse(
										p[9].Trim(),
										out int svn
									)
										? svn
										: 10,

								SoLuongTonVien =
									int.TryParse(
										p[10].Trim(),
										out int sl
									)
										? sl
										: 0
							}
						);
					}
				}


				// Nếu file cũ không đúng định dạng
				if (DanhSachThuoc.Count == 0)
				{
					TaoFileThuocMau();

					DocFileThuoc();
				}
			}
			catch
			{
				// Giữ cách hoạt động cũ
			}
		}


		public static void LuuFileThuoc()
		{
			List<string> lines =
				DanhSachThuoc
				.Select(t =>
					$"{t.MaThuoc}|" +
					$"{t.TenThuoc}|" +
					$"{t.ThanhPhan}|" +
					$"{t.CoSoSanXuat}|" +
					$"{t.DonViTinh}|" +
					$"{t.GiaHop}|" +
					$"{t.GiaVi}|" +
					$"{t.GiaVien}|" +
					$"{t.SoViTrongHop}|" +
					$"{t.SoVienTrongVi}|" +
					$"{t.SoLuongTonVien}"
				)
				.ToList();


			File.WriteAllLines(
				fileThuoc,
				lines,
				Encoding.UTF8
			);
		}


		private static void TaoFileThuocMau()
		{
			string data =
@"T001|Paracetamol 500mg|Paracetamol|Dược Hậu Giang|Hộp|35000|4000|500|10|10|8500
T002|Panadol Extra|Paracetamol, Caffeine|GlaxoSmithKline|Hộp|180000|15000|1500|12|10|14800
T003|Efferalgan 500mg (Sủi)|Paracetamol|UPSA SAS|Hộp|48000|25000|3000|2|8|1472
T004|Berocca Performance (Sủi)|Vitamin B, C, Zinc|Bayer|Tuýp|75000|0|7500|1|10|600
T005|Tiffy Dey|Paracetamol, Phenylephrine|Thai Nakorn Patana|Hộp|150000|6000|1800|25|4|2760
T006|Berberin 100mg|Berberin|Dược Hà Thành|Lọ|25000|0|250|1|100|5600";

			File.WriteAllText(
				fileThuoc,
				data,
				Encoding.UTF8
			);
		}


		// ================================================================
		// XÁC ĐỊNH CẤP VIP TỪ ĐIỂM
		//
		// 1 = Thành Viên
		// 2 = VIP Bạc
		// 3 = VIP Vàng
		// 4 = VIP Kim Cương
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
		// XỬ LÝ FILE KHÁCH HÀNG
		//
		// ĐỊNH DẠNG MỚI:
		//
		// SĐT | Họ tên | Điểm khả dụng | Cấp VIP | Ngày bắt đầu hạng
		//
		// Ví dụ:
		//
		// 0963555666|Bùi Thanh Lan|5000|4|2026-08-23 00:00:00
		//
		// Vẫn hỗ trợ file cũ 3 cột để tránh lỗi.
		// ================================================================
		private static void DocFileKhachHang()
		{
			DanhSachKhachHang.Clear();


			if (!File.Exists(fileKhachHang))
			{
				TaoFileKhachHangMau();
			}


			bool fileCuCanNangCap = false;


			try
			{
				string[] lines = File.ReadAllLines(
					fileKhachHang,
					Encoding.UTF8
				);


				foreach (var line in lines)
				{
					if (string.IsNullOrWhiteSpace(line))
						continue;


					string[] p = line.Split('|');


					if (p.Length < 3)
						continue;


					// =====================================================
					// THÔNG TIN CƠ BẢN
					// =====================================================
					string sdt =
						p[0].Trim();

					string hoTen =
						p[1].Trim();


					int diem =
						int.TryParse(
							p[2].Trim(),
							out int d
						)
							? d
							: 0;


					if (diem < 0)
						diem = 0;


					// =====================================================
					// CẤP VIP
					// =====================================================
					int capVip;


					if (p.Length >= 4 &&
						int.TryParse(
							p[3].Trim(),
							out int cap))
					{
						capVip = cap;
					}
					else
					{
						/*
						 * File cũ 3 cột.
						 *
						 * Xác định hạng ban đầu theo điểm.
						 */
						capVip =
							LayCapVipTheoDiem(diem);

						fileCuCanNangCap = true;
					}


					// Chặn dữ liệu sai
					if (capVip < 1)
						capVip = 1;

					if (capVip > 4)
						capVip = 4;


					// =====================================================
					// NGÀY BẮT ĐẦU CHU KỲ VIP
					// =====================================================
					DateTime ngayBatDauVip;


					if (p.Length >= 5 &&
						DateTime.TryParse(
							p[4].Trim(),
							out DateTime ngayVip))
					{
						ngayBatDauVip =
							ngayVip;
					}
					else
					{
						/*
						 * Chỉ file cũ mới vào đây.
						 *
						 * Lần đầu lấy ngày hiện tại.
						 * Sau đó file được lưu lại 5 cột.
						 */
						ngayBatDauVip =
							DateTime.Now;

						fileCuCanNangCap = true;
					}


					// =====================================================
					// TẠO ĐỐI TƯỢNG KHÁCH HÀNG
					// =====================================================
					KhachHang kh =
						new KhachHang
						{
							SoDienThoai =
								sdt,

							HoTen =
								hoTen,

							/*
							 * Điểm trong file là điểm khách
							 * đang còn sử dụng được.
							 *
							 * Setter này sẽ tạo BatchDiem.
							 */
							DiemTichLuy =
								diem
						};


					/*
					 * QUAN TRỌNG:
					 *
					 * DiemTichLuy setter có thể tự khởi tạo
					 * NgayThangHang.
					 *
					 * Vì vậy phải ghi lại đúng CapVip và
					 * NgayThangHang từ file SAU KHI set điểm.
					 */
					kh.CapVip =
						capVip;

					kh.NgayThangHang =
						ngayBatDauVip;


					DanhSachKhachHang.Add(kh);
				}


				// =====================================================
				// Nếu vô tình còn file cũ 3 cột
				// -> tự chuyển sang 5 cột.
				// =====================================================
				if (fileCuCanNangCap)
				{
					LuuFileKhachHang();
				}
			}
			catch (Exception ex)
			{
				System.Windows.Forms.MessageBox.Show(
					"Lỗi đọc file khách hàng: " +
					ex.Message
				);
			}
		}


		// ================================================================
		// LƯU FILE KHÁCH HÀNG
		//
		// QUAN TRỌNG:
		//
		// Phải lưu:
		// - Điểm khả dụng
		// - CapVip
		// - NgayThangHang
		//
		// Nếu không, đóng app sẽ mất thời hạn VIP.
		// ================================================================
		public static void LuuFileKhachHang()
		{
			try
			{
				List<string> lines =
					new List<string>();


				foreach (var kh in DanhSachKhachHang)
				{
					if (kh == null)
						continue;


					string line =
						$"{kh.SoDienThoai}|" +
						$"{kh.HoTen}|" +

						// Điểm khách còn được dùng
						$"{kh.DiemKhaDung}|" +

						// Cấp VIP hiện tại
						$"{kh.CapVip}|" +

// Ngày bắt đầu giữ hạng hiện tại
						$"{kh.NgayThangHang:yyyy-MM-dd}";

					lines.Add(line);
				}


				/*
				 * FIX:
				 *
				 * Code cũ:
				 *
				 * File.WriteAllLines("KhachHang.txt", ...)
				 *
				 * Nay dùng đúng fileKhachHang
				 * để toàn chương trình cùng một file.
				 */
				File.WriteAllLines(
					fileKhachHang,
					lines,
					Encoding.UTF8
				);
			}
			catch (Exception ex)
			{
				System.Windows.Forms.MessageBox.Show(
					"Lỗi lưu file khách hàng: " +
					ex.Message
				);
			}
		}


		// ================================================================
		// KHỞI TẠO DỮ LIỆU KHI APP CHẠY
		// ================================================================
		public static void KhoiTaoDuLieuApp()
		{
			/*
			 * 1. Đọc số dư điểm hiện tại + VIP từ khachhang.txt
			 */
			DocFileKhachHang();


			/*
			 * 2. Đọc lịch sử mua hàng.
			 *
			 * Lịch sử này được dùng để:
			 *
			 * - xét điểm trong chu kỳ VIP
			 * - lịch sử mua hàng
			 * - doanh thu
			 */
			DocFileDonHang();


			/*
			 * QUAN TRỌNG:
			 *
			 * KHÔNG GỌI:
			 *
			 * DongBoDiemTuLichSu();
			 *
			 * ở đây nữa.
			 *
			 * Vì điểm hiện tại đã được lưu trong khachhang.txt.
			 *
			 * Nếu replay lịch sử:
			 *
			 * - xóa 5000 điểm ban đầu
			 * - dựng lại điểm theo đơn
			 * - gây sai số dư
			 * - làm hỏng test VIP
			 */
		}


		// ================================================================
		// ĐỒNG BỘ LỊCH SỬ
		//
		// GIỮ LẠI TÊN HÀM để không làm hỏng code khác nếu đang gọi.
		//
		// Nhưng KHÔNG còn xóa điểm và replay ví điểm.
		// ================================================================
		public static void DongBoDiemTuLichSu()
		{
			if (DanhSachKhachHang == null)
			{
				DanhSachKhachHang =
					new List<KhachHang>();
			}


			if (DanhSachDonHang == null)
			{
				DanhSachDonHang =
					new List<LichSuMuaHang>();
			}


			foreach (var kh in DanhSachKhachHang)
			{
				if (kh == null)
					continue;


				/*
				 * Chỉ đồng bộ lịch sử giao dịch.
				 *
				 * KHÔNG:
				 *
				 * kh.DanhSachDiem.Clear();
				 * kh.TongDiemTichLuy = 0;
				 *
				 * nữa.
				 */
				kh.DanhSachLichSu =
					DanhSachDonHang
						.Where(
							d =>
								d != null &&
								d.SoDienThoai ==
								kh.SoDienThoai
						)
						.OrderBy(
							d => d.NgayMua
						)
						.ToList();
			}
		}


		// ================================================================
		// TẠO FILE KHÁCH HÀNG MẪU
		//
		// Bây giờ tạo luôn định dạng 5 cột.
		// ================================================================
		private static void TaoFileKhachHangMau()
		{
			string ngay =
				DateTime.Now.ToString(
					"yyyy-MM-dd HH:mm:ss"
				);


			string data =
				$"0912345678|Nguyễn Văn An|150|1|{ngay}\r\n" +
				$"0987654321|Trần Thị Bình|80|1|{ngay}\r\n" +
				$"0905123456|Lê Hoàng Cường|210|1|{ngay}";


			File.WriteAllText(
				fileKhachHang,
				data,
				Encoding.UTF8
			);
		}


		// ================================================================
		// XỬ LÝ FILE ĐƠN HÀNG
		// ================================================================
		private static void DocFileDonHang()
		{
			DanhSachDonHang.Clear();


			/*
			 * Rất quan trọng:
			 *
			 * Nếu hàm được gọi lại nhiều lần,
			 * phải clear lịch sử đang gắn trong khách trước.
			 *
			 * Nếu không cùng một hóa đơn sẽ bị add
			 * vào DanhSachLichSu nhiều lần.
			 */
			if (DanhSachKhachHang != null)
			{
				foreach (var kh in DanhSachKhachHang)
				{
					if (kh == null)
						continue;


					if (kh.DanhSachLichSu == null)
					{
						kh.DanhSachLichSu =
							new List<LichSuMuaHang>();
					}
					else
					{
						kh.DanhSachLichSu.Clear();
					}
				}
			}


			if (!File.Exists(fileDonHang))
				return;


			try
			{
				string[] lines =
					File.ReadAllLines(
						fileDonHang,
						Encoding.UTF8
					);


				foreach (var line in lines)
				{
					if (string.IsNullOrWhiteSpace(line))
						continue;


					string[] p =
						line.Split('|');


					if (p.Length >= 7)
					{
						var dh =
							new LichSuMuaHang
							{
								MaHoaDon =
									p[0].Trim(),

								NgayMua =
									DateTime.TryParse(
										p[1].Trim(),
										out DateTime dt
									)
										? dt
										: DateTime.Now,

								SoDienThoai =
									p[2].Trim(),

								TongTien =
									decimal.TryParse(
										p[3].Trim(),
										out decimal tt
									)
										? tt
										: 0,

								DiemCong =
									int.TryParse(
										p[4].Trim(),
										out int dc
									)
										? dc
										: 0,

								DiemTru =
									int.TryParse(
										p[5].Trim(),
										out int dtru
									)
										? dtru
										: 0,

								ChiTietDonHang =
									p[6].Trim()
							};


						// Thêm vào danh sách đơn hàng chung
						DanhSachDonHang.Add(dh);


						// =================================================
						// GẮN ĐƠN HÀNG VÀO KHÁCH
						//
						// Đây là dữ liệu dùng cho:
						// TinhDiemVipTrongKhoang()
						// =================================================
						var kh =
							DanhSachKhachHang
								.FirstOrDefault(
									k =>
										k != null &&
										k.SoDienThoai ==
										dh.SoDienThoai
								);


						if (kh != null)
						{
							if (kh.DanhSachLichSu == null)
							{
								kh.DanhSachLichSu =
									new List<LichSuMuaHang>();
							}


							kh.DanhSachLichSu.Add(dh);
						}
					}
				}
			}
			catch (Exception ex)
			{
				System.Windows.Forms.MessageBox.Show(
					"Lỗi đọc file đơn hàng: " +
					ex.Message
				);
			}
		}


		// ================================================================
		// LƯU ĐƠN HÀNG
		// GIỮ NGUYÊN ĐỊNH DẠNG
		// ================================================================
		public static void LuuFileDonHang()
		{
			List<string> lines =
				DanhSachDonHang
					.Where(d => d != null)
					.Select(
						d =>
							$"{d.MaHoaDon}|" +
							$"{d.NgayMua:yyyy-MM-dd HH:mm:ss}|" +
							$"{d.SoDienThoai}|" +
							$"{d.TongTien}|" +
							$"{d.DiemCong}|" +
							$"{d.DiemTru}|" +
							$"{d.ChiTietDonHang}"
					)
					.ToList();


			File.WriteAllLines(
				fileDonHang,
				lines,
				Encoding.UTF8
			);
		}


		// ================================================================
		// THÔNG TIN HẠNG
		//
		// GIỮ NGUYÊN HÀM để Form1 cũ vẫn compile.
		//
		// Sau này khi sửa Form1, hạng hiện tại sẽ lấy từ CapVip,
		// không lấy trực tiếp từ số điểm này nữa.
		// ================================================================
		public static (string Name, Color Color)
			GetTierInfo(int points)
		{
			if (points >= 5000)
			{
				return (
					"VIP KIM CƯƠNG",
					Color.FromArgb(0, 102, 204)
				);
			}


			if (points >= 2000)
			{
				return (
					"VIP VÀNG",
					Color.FromArgb(204, 153, 0)
				);
			}


			if (points >= 500)
			{
				return (
					"VIP BẠC",
					Color.FromArgb(128, 128, 128)
				);
			}


			return (
				"THÀNH VIÊN",
				Color.DarkGray
			);
		}
	}
}