using System;

namespace QuanLyNhaThuoc
{
	// Model Quản lý Thuốc
	public class Thuoc
	{
		public string MaThuoc { get; set; }
		public string TenThuoc { get; set; }
		public string ThanhPhan { get; set; }
		public string CoSoSanXuat { get; set; }

		// Mức giá tương ứng từng ĐVT
		public decimal GiaHop { get; set; }
		public decimal GiaVi { get; set; }
		public decimal GiaVien { get; set; }

		// Hệ số quy đổi (Ví dụ: 1 Hộp = 10 Vỉ, 1 Vỉ = 10 Viên -> 1 Hộp = 100 Viên)
		public int SoViTrongHop { get; set; } = 10;
		public int SoVienTrongVi { get; set; } = 10;

		// Quản lý tồn kho theo đơn vị nhỏ nhất (Tổng số Viên trong kho)
		public int SoLuongTonVien { get; set; }

		// Đơn vị mặc định hiển thị
		public string DonViTinh { get; set; } = "Hộp";

		// Đơn giá bán mặc định (trả về giá Hộp)
		public decimal GiaBan => GiaHop;

		// Chuỗi hiển thị tồn kho thông minh (Ví dụ: "8 Hộp 5 Vỉ")
		// Chuỗi hiển thị tồn kho thông minh chuẩn theo từng ĐVT (Hộp/Vỉ/Viên hoặc Tuýp/Lọ/Viên)
		public string TonKhoHienThi
		{
			get
			{
				string dvtChinh = string.IsNullOrWhiteSpace(DonViTinh) ? "Hộp" : DonViTinh.Trim();
				int tongVienPerUnit = SoViTrongHop * SoVienTrongVi;

				// Nếu không quy đổi lẻ
				if (tongVienPerUnit <= 1)
				{
					return $"{SoLuongTonVien} {dvtChinh}";
				}

				// Nếu là "Hộp" và có chia Vỉ (SoViTrongHop > 1)
				if (dvtChinh.Equals("Hộp", StringComparison.OrdinalIgnoreCase) && SoViTrongHop > 1)
				{
					int hop = SoLuongTonVien / tongVienPerUnit;
					int duVien = SoLuongTonVien % tongVienPerUnit;
					int vi = duVien / SoVienTrongVi;
					int vienLe = duVien % SoVienTrongVi;

					var parts = new System.Collections.Generic.List<string>();
					if (hop > 0 || (vi == 0 && vienLe == 0)) parts.Add($"{hop} Hộp");
					if (vi > 0) parts.Add($"{vi} Vỉ");
					if (vienLe > 0) parts.Add($"{vienLe} Viên");

					return string.Join(" ", parts);
				}
				else
				{
					// Dành riêng cho Tuýp, Lọ, Chai, Gói... (Chỉ gồm Tuýp/Lọ và Viên lẻ, không có Vỉ)
					int soLuongChinh = SoLuongTonVien / tongVienPerUnit;
					int vienLe = SoLuongTonVien % tongVienPerUnit;

					var parts = new System.Collections.Generic.List<string>();
					if (soLuongChinh > 0 || vienLe == 0) parts.Add($"{soLuongChinh} {dvtChinh}");
					if (vienLe > 0) parts.Add($"{vienLe} Viên");

					return string.Join(" ", parts);
				}
			}
		}

		// --- BỔ SUNG 2 THUỘC TÍNH CẦU NỐI TƯƠNG THÍCH VỚI FORM1 CŨ ---
		public int SoLuongTon
		{
			get => SoLuongTonVien;
			set => SoLuongTonVien = value;
		}

		public decimal DonGia
		{
			get => GiaHop > 0 ? GiaHop : (GiaVien > 0 ? GiaVien : GiaVi);
			set => GiaHop = value;
		}
	}

	// --- BỔ SUNG LỚP CHI TIẾT GIỎ HÀNG (SỬA LỖI CS0246) ---
	public class ChiTietGioHang
	{
		public Thuoc ThuocItem { get; set; }
		public string DonViChon { get; set; } // Tuýp, Lọ, Hộp, Vỉ, Viên...
		public int SoLuong { get; set; }

		// Tự động tính giá theo ĐVT được chọn
		public decimal DonGia
		{
			get
			{
				if (ThuocItem == null) return 0;
				if (DonViChon == "Vỉ") return ThuocItem.GiaVi > 0 ? ThuocItem.GiaVi : ThuocItem.GiaHop;
				if (DonViChon == "Viên") return ThuocItem.GiaVien > 0 ? ThuocItem.GiaVien : ThuocItem.GiaHop;
				return ThuocItem.GiaHop; // Đơn giá theo ĐVT gốc (Tuýp, Lọ, Hộp...)
			}
		}

		public decimal ThanhTien => DonGia * SoLuong;

		// Quy đổi ra tổng số VIÊN để trừ tồn kho chính xác
		public int TongSoVienCanTru
		{
			get
			{
				if (ThuocItem == null) return 0;
				if (DonViChon == "Viên")
					return SoLuong;
				if (DonViChon == "Vỉ")
					return SoLuong * ThuocItem.SoVienTrongVi;

				// Trường hợp ĐVT gốc (Hộp, Lọ, Tuýp...)
				return SoLuong * ThuocItem.SoViTrongHop * ThuocItem.SoVienTrongVi;
			}
		}
	}
}