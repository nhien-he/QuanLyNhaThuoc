using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace QuanLyNhaThuoc
{
	public class LoNhapKho
	{
		public string MaLo { get; set; }

		public string MaThuoc { get; set; }

		public string TenThuoc { get; set; }

		public string NhaCungCap { get; set; }

		public DateTime NgayNhap { get; set; }

		public DateTime HanSuDung { get; set; }

		public int SoLuongNhap { get; set; }

		public string DonViTinh { get; set; }

		public decimal DonGiaNhap { get; set; }

		public decimal ThanhTien
		{
			get
			{
				return SoLuongNhap * DonGiaNhap;
			}
		}

		public string TrangThaiHanSuDung
		{
			get
			{
				DateTime homNay = DateTime.Today;

				if (HanSuDung.Date < homNay)
					return "❌ Đã hết hạn";

				int soNgay =
					(HanSuDung.Date - homNay).Days;

				if (soNgay <= 30)
					return $"⚠️ Còn {soNgay} ngày";

				if (soNgay <= 90)
					return $"🟡 Còn {soNgay} ngày";

				return "✅ Còn hạn";
			}
		}
	}


	public static class QuanLyLoNhapKhoData
	{
		public static List<LoNhapKho> DanhSachLoNhap =
			new List<LoNhapKho>();


		private static string fileLoNhap =
			"lohang.txt";


		static QuanLyLoNhapKhoData()
		{
			DocFile();
		}


		// ========================================================
		// ĐỌC FILE LÔ NHẬP
		// ========================================================
		public static void DocFile()
		{
			DanhSachLoNhap.Clear();

			if (!File.Exists(fileLoNhap))
				return;


			try
			{
				string[] lines =
					File.ReadAllLines(
						fileLoNhap,
						Encoding.UTF8
					);


				foreach (string line in lines)
				{
					if (string.IsNullOrWhiteSpace(line))
						continue;


					string[] p =
						line.Split('|');


					/*
					 * 0 = Mã lô
					 * 1 = Mã thuốc
					 * 2 = Tên thuốc
					 * 3 = Nhà cung cấp
					 * 4 = Ngày nhập
					 * 5 = HSD
					 * 6 = Số lượng
					 * 7 = ĐVT
					 * 8 = Giá nhập
					 */
					if (p.Length < 9)
						continue;


					DateTime.TryParse(
						p[4].Trim(),
						out DateTime ngayNhap
					);

					DateTime.TryParse(
						p[5].Trim(),
						out DateTime hanSuDung
					);

					int.TryParse(
						p[6].Trim(),
						out int soLuong
					);

					decimal.TryParse(
						p[8].Trim(),
						out decimal giaNhap
					);


					DanhSachLoNhap.Add(
						new LoNhapKho
						{
							MaLo =
								p[0].Trim(),

							MaThuoc =
								p[1].Trim(),

							TenThuoc =
								p[2].Trim(),

							NhaCungCap =
								p[3].Trim(),

							NgayNhap =
								ngayNhap,

							HanSuDung =
								hanSuDung,

							SoLuongNhap =
								soLuong,

							DonViTinh =
								p[7].Trim(),

							DonGiaNhap =
								giaNhap
						}
					);
				}
			}
			catch
			{
				// Không làm ảnh hưởng chương trình chính
			}
		}


		// ========================================================
		// LƯU FILE
		// ========================================================
		public static void LuuFile()
		{
			try
			{
				List<string> lines =
					DanhSachLoNhap
					.Select(lo =>
						$"{lo.MaLo}|" +
						$"{lo.MaThuoc}|" +
						$"{lo.TenThuoc}|" +
						$"{lo.NhaCungCap}|" +
						$"{lo.NgayNhap:yyyy-MM-dd HH:mm:ss}|" +
						$"{lo.HanSuDung:yyyy-MM-dd}|" +
						$"{lo.SoLuongNhap}|" +
						$"{lo.DonViTinh}|" +
						$"{lo.DonGiaNhap}"
					)
					.ToList();


				File.WriteAllLines(
					fileLoNhap,
					lines,
					Encoding.UTF8
				);
			}
			catch
			{
				// Không làm hỏng phần tồn kho hiện tại
			}
		}


		// ========================================================
		// KIỂM TRA TRÙNG MÃ LÔ
		// ========================================================
		public static bool MaLoDaTonTai(string maLo)
		{
			if (string.IsNullOrWhiteSpace(maLo))
				return false;


			return DanhSachLoNhap.Any(
				x =>
					x != null &&
					string.Equals(
						x.MaLo,
						maLo.Trim(),
						StringComparison.OrdinalIgnoreCase
					)
			);
		}


		// ========================================================
		// SINH MÃ LÔ MỚI
		//
		// Ví dụ:
		// LOT-T001-20260825-001
		// LOT-T001-20260825-002
		// LOT-T002-20260825-001
		// ========================================================
		public static string TaoMaLoMoi(
			string maThuoc,
			DateTime ngayNhap)
		{
			if (string.IsNullOrWhiteSpace(maThuoc))
				maThuoc = "THUOC";


			string prefix =
				$"LOT-{maThuoc.Trim().ToUpper()}-" +
				$"{ngayNhap:yyyyMMdd}-";


			int soThuTuLonNhat = 0;


			foreach (var lo in DanhSachLoNhap)
			{
				if (lo == null ||
					string.IsNullOrWhiteSpace(lo.MaLo))
					continue;


				if (!lo.MaLo.StartsWith(
					prefix,
					StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}


				string phanSo =
					lo.MaLo.Substring(
						prefix.Length
					);


				if (int.TryParse(
					phanSo,
					out int stt))
				{
					if (stt > soThuTuLonNhat)
						soThuTuLonNhat = stt;
				}
			}


			int sttMoi =
				soThuTuLonNhat + 1;


			return prefix +
				   sttMoi.ToString("D3");
		}


		// ========================================================
		// THÊM LÔ
		// ========================================================
		public static void ThemLo(LoNhapKho lo)
		{
			if (lo == null)
				return;


			DanhSachLoNhap.Add(lo);

			LuuFile();
		}
	}
}