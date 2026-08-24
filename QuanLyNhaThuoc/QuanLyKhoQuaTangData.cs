using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace QuanLyNhaThuoc
{
	public class PhieuNhapQuaTang
	{
		public string MaPhieu { get; set; }

		public string MaQua { get; set; }

		public string TenQua { get; set; }

		public string NguonCap { get; set; }

		public DateTime NgayNhap { get; set; }

		public int SoLuongNhap { get; set; }

		public DateTime? HanSuDung { get; set; }

		public string GhiChu { get; set; }

		public string TrangThaiHSD
		{
			get
			{
				// Quà không có HSD
				if (!HanSuDung.HasValue)
					return "Không áp dụng";

				DateTime homNay = DateTime.Today;

				if (HanSuDung.Value.Date < homNay)
					return "❌ Đã hết hạn";

				int soNgay =
					(HanSuDung.Value.Date - homNay).Days;

				if (soNgay <= 30)
					return $"⚠️ Còn {soNgay} ngày";

				if (soNgay <= 90)
					return $"🟡 Còn {soNgay} ngày";

				return "✅ Còn hạn";
			}
		}
	}


	public static class QuanLyKhoQuaTangData
	{
		private static string fileQuaTang =
			"quatang.txt";

		private static string fileNhapQua =
			"nhapqua.txt";


		public static List<PhieuNhapQuaTang> DanhSachNhapQua =
			new List<PhieuNhapQuaTang>();


		private static bool daKhoiTao = false;


		// ========================================================
		// KHỞI TẠO
		// ========================================================
		public static void KhoiTao()
		{
			if (daKhoiTao)
				return;

			daKhoiTao = true;

			DocTonQuaTang();
			DocLichSuNhapQua();
		}


		// ========================================================
		// ĐỌC TỒN QUÀ
		//
		// quatang.txt:
		//
		// Mã|Tên|Điểm cần|Trị giá|Tồn
		// ========================================================
		private static void DocTonQuaTang()
		{
			/*
			 * LẦN ĐẦU CHƯA CÓ FILE:
			 * lấy danh sách quà hard-code hiện tại làm dữ liệu gốc
			 * rồi lưu xuống quatang.txt.
			 */
			if (!File.Exists(fileQuaTang))
			{
				LuuTonQua();

				return;
			}


			try
			{
				string[] lines =
					File.ReadAllLines(
						fileQuaTang,
						Encoding.UTF8
					);


				foreach (string line in lines)
				{
					if (string.IsNullOrWhiteSpace(line))
						continue;


					string[] p =
						line.Split('|');


					if (p.Length < 5)
						continue;


					string maQua =
						p[0].Trim();


					QuaTang qua =
						QuanLyQuaTangData
						.DanhSachQua
						.FirstOrDefault(q =>
							q != null &&
							q.MaQua == maQua
						);


					if (qua == null)
						continue;


					if (int.TryParse(
						p[2].Trim(),
						out int diemCan))
					{
						qua.DiemCan = diemCan;
					}


					if (decimal.TryParse(
						p[3].Trim(),
						out decimal triGia))
					{
						qua.TriGia = triGia;
					}


					if (int.TryParse(
						p[4].Trim(),
						out int ton))
					{
						qua.SoLuongTon =
							Math.Max(0, ton);
					}
				}
			}
			catch
			{
				// Không làm hỏng ứng dụng chính
			}
		}


		// ========================================================
		// LƯU TỒN QUÀ
		// ========================================================
		public static void LuuTonQua()
		{
			try
			{
				List<string> lines =
					QuanLyQuaTangData
					.DanhSachQua
					.Where(q => q != null)
					.Select(q =>
						$"{q.MaQua}|" +
						$"{q.TenSanPham}|" +
						$"{q.DiemCan}|" +
						$"{q.TriGia}|" +
						$"{q.SoLuongTon}"
					)
					.ToList();


				File.WriteAllLines(
					fileQuaTang,
					lines,
					Encoding.UTF8
				);
			}
			catch
			{
			}
		}


		// ========================================================
		// ĐỌC LỊCH SỬ NHẬP QUÀ
		// ========================================================
		private static void DocLichSuNhapQua()
		{
			DanhSachNhapQua.Clear();


			if (!File.Exists(fileNhapQua))
				return;


			try
			{
				string[] lines =
					File.ReadAllLines(
						fileNhapQua,
						Encoding.UTF8
					);


				foreach (string line in lines)
				{
					if (string.IsNullOrWhiteSpace(line))
						continue;


					string[] p =
						line.Split('|');


					/*
					 * 0 = Mã phiếu
					 * 1 = Mã quà
					 * 2 = Tên quà
					 * 3 = Nguồn cấp
					 * 4 = Ngày nhập
					 * 5 = Số lượng
					 * 6 = HSD
					 * 7 = Ghi chú
					 */
					if (p.Length < 8)
						continue;


					DateTime.TryParse(
						p[4].Trim(),
						out DateTime ngayNhap
					);


					int.TryParse(
						p[5].Trim(),
						out int sl
					);


					DateTime? hsd = null;


					if (!string.IsNullOrWhiteSpace(
						p[6]))
					{
						if (DateTime.TryParse(
							p[6].Trim(),
							out DateTime ngayHsd))
						{
							hsd = ngayHsd;
						}
					}


					DanhSachNhapQua.Add(
						new PhieuNhapQuaTang
						{
							MaPhieu =
								p[0].Trim(),

							MaQua =
								p[1].Trim(),

							TenQua =
								p[2].Trim(),

							NguonCap =
								p[3].Trim(),

							NgayNhap =
								ngayNhap,

							SoLuongNhap =
								sl,

							HanSuDung =
								hsd,

							GhiChu =
								p[7].Trim()
						}
					);
				}
			}
			catch
			{
			}
		}


		// ========================================================
		// LƯU LỊCH SỬ
		// ========================================================
		private static void LuuLichSuNhapQua()
		{
			try
			{
				List<string> lines =
					DanhSachNhapQua
					.Where(x => x != null)
					.Select(x =>
						$"{x.MaPhieu}|" +
						$"{x.MaQua}|" +
						$"{x.TenQua}|" +
						$"{x.NguonCap}|" +
						$"{x.NgayNhap:yyyy-MM-dd HH:mm:ss}|" +
						$"{x.SoLuongNhap}|" +
						$"{(x.HanSuDung.HasValue ? x.HanSuDung.Value.ToString("yyyy-MM-dd") : "")}|" +
						$"{x.GhiChu}"
					)
					.ToList();


				File.WriteAllLines(
					fileNhapQua,
					lines,
					Encoding.UTF8
				);
			}
			catch
			{
			}
		}


		// ========================================================
		// NHẬP QUÀ
		// ========================================================
		public static bool NhapQua(
			QuaTang qua,
			int soLuong,
			string nguonCap,
			DateTime? hanSuDung,
			string ghiChu)
		{
			if (qua == null ||
				soLuong <= 0)
			{
				return false;
			}


			// Tăng tồn
			qua.SoLuongTon += soLuong;


			PhieuNhapQuaTang phieu =
				new PhieuNhapQuaTang
				{
					MaPhieu =
						"NQ" +
						DateTime.Now.ToString(
							"yyMMddHHmmssfff"
						),

					MaQua =
						qua.MaQua,

					TenQua =
						qua.TenSanPham,

					NguonCap =
						nguonCap,

					NgayNhap =
						DateTime.Now,

					SoLuongNhap =
						soLuong,

					HanSuDung =
						hanSuDung,

					GhiChu =
						ghiChu ?? ""
				};


			DanhSachNhapQua.Add(
				phieu
			);


			// Lưu cả tồn + lịch sử
			LuuTonQua();

			LuuLichSuNhapQua();


			return true;
		}
	}
}