using System;
using System.Collections.Generic;
using System.Linq;

namespace QuanLyNhaThuoc
{
	public class LichSuMuaHang
	{
		public string MaHoaDon { get; set; }
		public DateTime NgayMua { get; set; }
		public string SoDienThoai { get; set; }
		public string MaHangHoa { get; set; }
		public decimal ThanhTien { get; set; }

		public decimal TongTien
		{
			get => ThanhTien;
			set => ThanhTien = value;
		}

		public int DiemCong { get; set; }
		public int DiemTru { get; set; }
		public string GhiChu { get; set; }

		public string ChiTietDonHang
		{
			get => GhiChu;
			set => GhiChu = value;
		}
	}


	// ================================================================
	// LÔ ĐIỂM
	// ================================================================
	public class BatchDiem
	{
		public int SoDiem { get; set; }
		public DateTime NgayHetHan { get; set; }

		// Điểm còn hạn khi:
		// - Số điểm > 0
		// - Chưa tới đúng thời điểm hết hạn
		public bool IsConHan
		{
			get
			{
				return SoDiem > 0 && NgayHetHan > DateTime.Now;
			}
		}

		// Còn hạn và <= 30 ngày thì cảnh báo
		public bool IsSapHetHan
		{
			get
			{
				if (!IsConHan)
					return false;

				return (NgayHetHan - DateTime.Now).TotalDays <= 30;
			}
		}

		public int SoNgayConLai
		{
			get
			{
				if (!IsConHan)
					return 0;

				double soNgay = (NgayHetHan - DateTime.Now).TotalDays;

				return Math.Max(
					1,
					(int)Math.Ceiling(soNgay)
				);
			}
		}

		public string ConLai
		{
			get
			{
				return IsConHan
					? $"{SoNgayConLai} ngày"
					: "0 ngày";
			}
		}

		public string TrangThai
		{
			get
			{
				if (!IsConHan)
					return "❌ Đã hết hạn";

				if (IsSapHetHan)
					return $"⚠️ Sắp hết hạn (còn {SoNgayConLai} ngày)";

				return $"✅ Còn hạn (còn {SoNgayConLai} ngày)";
			}
		}
	}


	// ================================================================
	// KHÁCH HÀNG
	// ================================================================
	public class KhachHang
	{
		public string SoDienThoai { get; set; }
		public string HoTen { get; set; }

		public List<LichSuMuaHang> DanhSachLichSu { get; set; }
			= new List<LichSuMuaHang>();

		public List<BatchDiem> DanhSachDiem { get; set; }
			= new List<BatchDiem>();


		// ============================================================
		// THÔNG TIN ĐỔI QUÀ
		// ============================================================
		public int SoLanDoiHomNay { get; set; } = 0;

		public DateTime NgayDoiGanNhat { get; set; }
			= DateTime.MinValue;


		// ============================================================
		// THÔNG TIN ĐIỂM
		// ============================================================

		// Tổng điểm khách từng được cộng.
		// Không giảm khi đổi quà/trừ điểm.
		public int TongDiemTichLuy { get; set; }

		// Dùng để tương thích với code cũ.
		public DateTime NgayKhoiTao { get; set; }
			= DateTime.Now;


		// ============================================================
		// HỆ THỐNG VIP MỚI
		// ============================================================

		/*
		 * CẤP VIP:
		 *
		 * 1 = Thành Viên
		 * 2 = VIP Bạc
		 * 3 = VIP Vàng
		 * 4 = VIP Kim Cương
		 */
		public int CapVip { get; set; } = 1;


		/*
		 * Ngày bắt đầu chu kỳ giữ hạng hiện tại.
		 *
		 * Ví dụ:
		 *
		 * Kim Cương từ:
		 * 23/08/2026
		 *
		 * => hạn:
		 * 23/08/2027
		 */
		public DateTime NgayThangHang { get; set; }
			= DateTime.MinValue;


		// ============================================================
		// TÊN HẠNG VIP
		// ============================================================
		public string HangVip
		{
			get
			{
				return GetTenHangByCap(CapVip);
			}

			set
			{
				int capMoi = GetCap(value);

				/*
				 * QUAN TRỌNG:
				 *
				 * Không cho code bên ngoài tự ý hạ hạng.
				 *
				 * Ví dụ Form1 cũ có:
				 *
				 * kh.HangVip = GetVipTier(...).Name;
				 *
				 * Nếu khách đang Kim Cương mà điểm đang tiêu
				 * còn thấp thì code cũ có thể gán Thành Viên.
				 *
				 * Bây giờ việc HẠ HẠNG chỉ được thực hiện
				 * trong KiemTraCapNhatHangVipLongChau().
				 */

				if (NgayThangHang == DateTime.MinValue)
				{
					CapVip = capMoi;
					return;
				}

				// Chỉ cho phép gán từ bên ngoài nếu đó là THĂNG HẠNG
				if (capMoi > CapVip)
				{
					CapVip = capMoi;
					NgayThangHang = DateTime.Today;
				}
			}
		}


		// ============================================================
		// HẠN GIỮ HẠNG
		// ============================================================
		public DateTime HanGiuHang
		{
			get
			{
				if (NgayThangHang == DateTime.MinValue)
					return DateTime.MinValue;

				return NgayThangHang.Date.AddYears(1);
			}
		}
		// ============================================================
		// ĐIỂM KHẢ DỤNG
		//
		// Đây là điểm khách còn có thể:
		// - Đổi quà
		// - Giảm tiền
		//
		// KHÔNG dùng điểm này để quyết định hạ hạng VIP.
		// ============================================================
		public int DiemKhaDung
		{
			get
			{
				if (DanhSachDiem == null ||
					DanhSachDiem.Count == 0)
				{
					return 0;
				}

				return DanhSachDiem
					.Where(b => b != null && b.IsConHan)
					.Sum(b => b.SoDiem);
			}
		}


		// ============================================================
		// ĐIỂM 365 NGÀY
		//
		// GIỮ LẠI để Form1/code cũ không bị lỗi compile.
		//
		// Nhưng hệ thống VIP MỚI KHÔNG còn dùng thuộc tính này
		// để hạ hạng.
		// ============================================================
		public int DiemXetHang365Ngay
		{
			get
			{
				DateTime ngay1NamTruoc =
					DateTime.Now.AddYears(-1);

				int diemLichSu365 = 0;

				if (DanhSachLichSu != null)
				{
					diemLichSu365 = DanhSachLichSu
						.Where(h =>
							h != null &&
							h.NgayMua >= ngay1NamTruoc)
						.Sum(h => h.DiemCong);
				}

				int diemBanDauChuaCoLichSu = 0;

				if (NgayKhoiTao >= ngay1NamTruoc)
				{
					int tongDiemTuLichSu = 0;

					if (DanhSachLichSu != null)
					{
						tongDiemTuLichSu =
							DanhSachLichSu.Sum(
								h => h != null
									? h.DiemCong
									: 0
							);
					}

					diemBanDauChuaCoLichSu =
						Math.Max(
							0,
							TongDiemTichLuy -
							tongDiemTuLichSu
						);
				}

				return diemBanDauChuaCoLichSu
					+ diemLichSu365;
			}
		}


		// ============================================================
		// ĐIỂM TÍCH LŨY
		//
		// Giữ tương thích với code đọc khachhang.txt hiện tại.
		// ============================================================
		public int DiemTichLuy
		{
			get
			{
				return DiemXetHang365Ngay;
			}

			set
			{
				if (value < 0)
					value = 0;

				TongDiemTichLuy = value;

				/*
				 * Khi đọc dữ liệu khách hàng lần đầu từ file TXT,
				 * tạo lô điểm tương ứng.
				 */
				if (value > 0 &&
					(DanhSachDiem == null ||
					 DanhSachDiem.Count == 0))
				{
					NgayKhoiTao = DateTime.Now;

					DanhSachDiem =
						new List<BatchDiem>();


					/*
					 * GIỮ NGUYÊN CƠ CHẾ DEMO CỦA BẠN:
					 *
					 * Nếu >= 300 điểm:
					 * - 30 điểm sắp hết hạn sau 15 ngày
					 * - số còn lại hết hạn sau 1 năm
					 */
					if (value >= 300)
					{
						int diemSapHetHan = 30;

						DanhSachDiem.Add(
							new BatchDiem
							{
								SoDiem = diemSapHetHan,
								NgayHetHan =
									DateTime.Now.AddDays(15)
							}
						);

						DanhSachDiem.Add(
							new BatchDiem
							{
								SoDiem =
									value - diemSapHetHan,

								NgayHetHan =
									DateTime.Now.AddYears(1)
							}
						);
					}
					else
					{
						DanhSachDiem.Add(
							new BatchDiem
							{
								SoDiem = value,

								NgayHetHan =
									DateTime.Now.AddYears(1)
							}
						);
					}
				}


				/*
				 * CHỈ KHỞI TẠO HẠNG VIP LẦN ĐẦU.
				 *
				 * Tuyệt đối không cập nhật lại hạng mỗi lần
				 * DiemTichLuy thay đổi.
				 */
				if (NgayThangHang == DateTime.MinValue)
				{
					CapVip =
						GetCapTheoDiem(value);

					NgayThangHang =
						DateTime.Today;
				}
			}
		}


		// ============================================================
		// THÔNG BÁO ĐIỂM SẮP HẾT HẠN
		// ============================================================
		public string ThongBaoHetHan
		{
			get
			{
				if (DanhSachDiem == null ||
					DanhSachDiem.Count == 0)
				{
					return "An toàn";
				}

				bool coDiemSapHetHan =
					DanhSachDiem.Any(
						b => b != null &&
							 b.IsSapHetHan
					);

				return coDiemSapHetHan
					? "⚠️"
					: "An toàn";
			}
		}


		// ============================================================
		// CỘNG ĐIỂM
		// ============================================================
		public void CongDiem(
			int soDiemMoi,
			int soNamHieuLuc = 1)
		{
			if (soDiemMoi <= 0)
				return;

			if (DanhSachDiem == null)
			{
				DanhSachDiem =
					new List<BatchDiem>();
			}

			TongDiemTichLuy += soDiemMoi;

			DanhSachDiem.Add(
				new BatchDiem
				{
					SoDiem = soDiemMoi,

					NgayHetHan =
						DateTime.Now
							.AddYears(soNamHieuLuc)
				}
			);


			/*
			 * Sau khi cộng điểm, kiểm tra xem khách
			 * có đủ điều kiện THĂNG HẠNG hay không.
			 *
			 * Lịch sử đơn mua hàng phải được thêm vào
			 * DanhSachLichSu trước hoặc ngay trong quá trình
			 * thanh toán.
			 */
			KiemTraCapNhatHangVipLongChau();
		}


		// ============================================================
		// TRỪ ĐIỂM FIFO
		//
		// Ưu tiên lô sắp hết hạn trước.
		//
		// KHÔNG thay đổi CapVip.
		// KHÔNG thay đổi NgayThangHang.
		// ============================================================
		public void TruDiem(int diemTru)
		{
			if (diemTru <= 0)
				return;

			int diemCanTru = diemTru;

			if (DanhSachDiem == null)
				return;

			var cacLoConHan =
				DanhSachDiem
					.Where(
						b => b != null &&
							 b.IsConHan)
					.OrderBy(
						b => b.NgayHetHan)
					.ToList();

			foreach (var batch in cacLoConHan)
			{
				if (diemCanTru <= 0)
					break;

				if (batch.SoDiem >= diemCanTru)
				{
					batch.SoDiem -= diemCanTru;

					diemCanTru = 0;

					break;
				}
				else
				{
					diemCanTru -= batch.SoDiem;

					batch.SoDiem = 0;
				}
			}

			// Xóa các lô đã dùng hết
			DanhSachDiem.RemoveAll(
				b => b == null ||
					 b.SoDiem <= 0
			);
		}


		// ============================================================
		// KHỞI TẠO HẠNG VIP BAN ĐẦU
		//
		// Dùng khi đọc khachhang.txt.
		//
		// Ví dụ:
		// 5000 điểm => Kim Cương
		// ngày bắt đầu => 23/08/2026
		// ============================================================
		public void KhoiTaoHangVipBanDau(
	int diemBanDau,
	DateTime ngayBatDau)
		{
			if (diemBanDau < 0)
				diemBanDau = 0;

			CapVip =
				GetCapTheoDiem(diemBanDau);

			if (ngayBatDau == DateTime.MinValue)
			{
				NgayThangHang = DateTime.Today;
			}
			else
			{
				// Bỏ phần giờ, chỉ lấy ngày
				NgayThangHang = ngayBatDau.Date;
			}
		}

		// ============================================================
		// TÍNH ĐIỂM KIẾM ĐƯỢC TRONG MỘT KHOẢNG THỜI GIAN
		//
		// Chỉ tính DiemCong từ mua hàng.
		// DiemTru không ảnh hưởng việc xét VIP.
		// ============================================================
		public int TinhDiemVipTrongKhoang(
			DateTime tuNgay,
			DateTime denNgay)
		{
			if (DanhSachLichSu == null ||
				DanhSachLichSu.Count == 0)
			{
				return 0;
			}

			return DanhSachLichSu
				.Where(
					h =>
						h != null &&
						h.DiemCong > 0 &&
						h.NgayMua >= tuNgay &&
						h.NgayMua < denNgay
				)
				.Sum(h => h.DiemCong);
		}


		// ============================================================
		// ĐIỂM ĐÃ KIẾM ĐƯỢC TRONG CHU KỲ VIP HIỆN TẠI
		//
		// Ví dụ:
		//
		// Chu kỳ:
		// 23/08/2026 -> 23/08/2027
		//
		// Hôm nay:
		// 20/12/2026
		//
		// => tính điểm mua từ 23/08 tới 20/12.
		// ============================================================
		public int DiemVipChuKyHienTai
		{
			get
			{
				if (NgayThangHang ==
					DateTime.MinValue)
				{
					return 0;
				}

				DateTime denNgay =
					DateTime.Now.AddTicks(1);

				return TinhDiemVipTrongKhoang(
					NgayThangHang,
					denNgay
				);
			}
		}


		// ============================================================
		// ĐIỂM CẦN DUY TRÌ HẠNG HIỆN TẠI
		// ============================================================
		public int DiemCanDuyTriHangHienTai
		{
			get
			{
				return GetDiemCanDuyTri(CapVip);
			}
		}


		// ============================================================
		// SỐ ĐIỂM CÒN THIẾU ĐỂ DUY TRÌ
		// ============================================================
		public int DiemConThieuDeDuyTri
		{
			get
			{
				int diemCan =
					DiemCanDuyTriHangHienTai;

				int daCo =
					DiemVipChuKyHienTai;

				return Math.Max(
					0,
					diemCan - daCo
				);
			}
		}


		// ============================================================
		// ĐIỀU KIỆN DUY TRÌ HẠNG
		//
		// MỐC ĐỒNG BỘ VỚI FORM1:
		//
		// Bạc        = 500 điểm/năm
		// Vàng       = 2.000 điểm/năm
		// Kim Cương  = 5.000 điểm/năm
		// ============================================================
		private int GetDiemCanDuyTri(int cap)
		{
			switch (cap)
			{
				case 4:
					return 5000;

				case 3:
					return 2000;

				case 2:
					return 500;

				default:
					return 0;
			}
		}


		// ============================================================
		// XÁC ĐỊNH CẤP VIP THEO ĐIỂM
		//
		// Chỉ dùng để:
		// - Xác định hạng ban đầu
		// - Xác định thăng hạng
		//
		// KHÔNG dùng trực tiếp để hạ hạng.
		// ============================================================
		private int GetCapTheoDiem(int diem)
		{
			if (diem >= 5000)
				return 4;

			if (diem >= 2000)
				return 3;

			if (diem >= 500)
				return 2;

			return 1;
		}


		// ============================================================
		// HỆ THỐNG XÉT HẠNG VIP
		//
		// QUY TẮC:
		//
		// 1. Một hạng được bảo lưu đúng 1 năm.
		//
		// 2. Trong một năm:
		//    - Đủ điểm duy trì => giữ nguyên hạng.
		//    - Không đủ => chỉ hạ 1 bậc.
		//
		// 3. Nếu đủ điểm lên hạng cao hơn:
		//    => thăng ngay.
		//    => bắt đầu lại 1 năm từ ngày thăng hạng.
		//
		// 4. Đổi quà/trừ điểm không làm hạ hạng.
		//
		// Trả true nếu trạng thái VIP/ngày chu kỳ có thay đổi.
		// ============================================================
		public bool KiemTraCapNhatHangVipLongChau()
		{
			bool coThayDoi = false;

			// VIP CHỈ XÉT THEO NGÀY
			DateTime homNay = DateTime.Today;


			// ============================================================
			// 1. BẢO VỆ CẤP VIP
			// ============================================================
			if (CapVip < 1)
			{
				CapVip = 1;
				coThayDoi = true;
			}

			if (CapVip > 4)
			{
				CapVip = 4;
				coThayDoi = true;
			}


			// ============================================================
			// 2. CHƯA CÓ NGÀY BẮT ĐẦU HẠNG
			// ============================================================
			if (NgayThangHang == DateTime.MinValue)
			{
				CapVip = GetCapTheoDiem(
					Math.Max(TongDiemTichLuy, DiemKhaDung)
				);

				NgayThangHang = homNay;

				return true;
			}


			// ============================================================
			// 3. BỎ GIỜ / PHÚT / GIÂY
			// ============================================================
			DateTime ngayBatDau = NgayThangHang.Date;

			if (NgayThangHang != ngayBatDau)
			{
				NgayThangHang = ngayBatDau;
				coThayDoi = true;
			}


			// ============================================================
			// 4. XÉT CÁC CHU KỲ ĐÃ HẾT
			// ============================================================
			while (homNay >= NgayThangHang.Date.AddYears(1))
			{
				DateTime tuNgay = NgayThangHang.Date;

				DateTime denNgay =
					tuNgay.AddYears(1);


				// Điểm kiếm được trong đúng chu kỳ vừa kết thúc
				int diemTrongKy =
					TinhDiemVipTrongKhoang(
						tuNgay,
						denNgay
					);


				int capCu = CapVip;


				// ========================================================
				// XÉT DUY TRÌ HẠNG
				// ========================================================
				switch (capCu)
				{
					case 4:
						// Kim Cương cần 5000
						if (diemTrongKy < 5000)
						{
							CapVip = 3;
						}
						break;


					case 3:
						// Vàng cần 2000
						if (diemTrongKy < 2000)
						{
							CapVip = 2;
						}
						break;


					case 2:
						// Bạc cần 500
						if (diemTrongKy < 500)
						{
							CapVip = 1;
						}
						break;


					case 1:
					default:
						CapVip = 1;
						break;
				}


				if (CapVip != capCu)
				{
					coThayDoi = true;
				}


				// ========================================================
				// DÙ GIỮ HẠNG HAY HẠ HẠNG
				// ĐỀU BẮT ĐẦU CHU KỲ MỚI
				// ========================================================
				NgayThangHang = denNgay.Date;

				coThayDoi = true;
			}


			// ============================================================
			// 5. KIỂM TRA THĂNG HẠNG TRONG CHU KỲ MỚI
			// ============================================================
			int diemKyHienTai =
				TinhDiemVipTrongKhoang(
					NgayThangHang.Date,
					DateTime.Now.AddTicks(1)
				);


			int capDatDuoc =
				GetCapTheoDiem(
					diemKyHienTai
				);


			// CHỈ THĂNG, TUYỆT ĐỐI KHÔNG HẠ Ở ĐÂY
			if (capDatDuoc > CapVip)
			{
				CapVip = capDatDuoc;

				NgayThangHang = homNay;

				coThayDoi = true;
			}


			return coThayDoi;
		}

		// ============================================================
		// TÊN HẠNG THEO CẤP
		// ============================================================
		private string GetTenHangByCap(int cap)
		{
			switch (cap)
			{
				case 4:
					return "💎 VIP Kim Cương";

				case 3:
					return "🥇 VIP Vàng";

				case 2:
					return "🥈 VIP Bạc";

				default:
					return "🥉 Thành Viên";
			}
		}


		// ============================================================
		// HÀM CŨ - GIỮ TƯƠNG THÍCH
		// ============================================================
		private string GetTenHang(int diem)
		{
			return GetTenHangByCap(
				GetCapTheoDiem(diem)
			);
		}


		private int GetCap(string tenHang)
		{
			if (string.IsNullOrWhiteSpace(tenHang))
				return 1;

			if (tenHang.Contains("Kim Cương"))
				return 4;

			if (tenHang.Contains("Vàng"))
				return 3;

			if (tenHang.Contains("Bạc"))
				return 2;

			return 1;
		}
	}


	// ================================================================
	// QUÀ TẶNG
	// ================================================================
	public class QuaTang
	{
		public string MaQua { get; set; }
		public string TenSanPham { get; set; }
		public int DiemCan { get; set; }
		public decimal TriGia { get; set; }
		public int SoLuongTon { get; set; }
	}


	public static class QuanLyQuaTangData
	{
		public static List<QuaTang> DanhSachQua =
			new List<QuaTang>
		{
			new QuaTang
			{
				MaQua = "Q01",
				TenSanPham = "Cồn y tế 70 độ sát khuẩn (Chai 100ml)",
				DiemCan = 500,
				TriGia = 8000,
				SoLuongTon = 60
			},

			new QuaTang
			{
				MaQua = "Q02",
				TenSanPham = "Chai Nước muối sinh lý 0.9% (500ml)",
				DiemCan = 700,
				TriGia = 10000,
				SoLuongTon = 80
			},

			new QuaTang
			{
				MaQua = "Q03",
				TenSanPham = "Bịch Khẩu trang y tế 4 lớp (10 cái)",
				DiemCan = 700,
				TriGia = 10000,
				SoLuongTon = 100
			},

			new QuaTang
			{
				MaQua = "Q04",
				TenSanPham = "Hộp Băng cá nhân Urgo Waterproof (20 miếng)",
				DiemCan = 1000,
				TriGia = 15000,
				SoLuongTon = 50
			},

			new QuaTang
			{
				MaQua = "Q05",
				TenSanPham = "Hộp Gạc y tế tiệt trùng (10 miếng)",
				DiemCan = 1000,
				TriGia = 15000,
				SoLuongTon = 45
			},

			new QuaTang
			{
				MaQua = "Q06",
				TenSanPham = "Chai Gel rửa tay khô sát khuẩn 50ml",
				DiemCan = 1400,
				TriGia = 20000,
				SoLuongTon = 35
			},

			new QuaTang
			{
				MaQua = "Q07",
				TenSanPham = "Tuýp Vitamin C sủi BeroVita (20 viên)",
				DiemCan = 1800,
				TriGia = 25000,
				SoLuongTon = 30
			},

			new QuaTang
			{
				MaQua = "Q08",
				TenSanPham = "Chai Dầu gió xanh / Khuynh diệp 25ml",
				DiemCan = 2100,
				TriGia = 30000,
				SoLuongTon = 25
			},

			new QuaTang
			{
				MaQua = "Q09",
				TenSanPham = "Hộp Miếng dán hạ sốt Fever-Cool (4 miếng)",
				DiemCan = 2200,
				TriGia = 32000,
				SoLuongTon = 30
			},

			new QuaTang
			{
				MaQua = "Q10",
				TenSanPham = "Chai Nước súc miệng Listerine 250ml",
				DiemCan = 2500,
				TriGia = 35000,
				SoLuongTon = 25
			},

			new QuaTang
			{
				MaQua = "Q11",
				TenSanPham = "Hộp Khẩu trang y tế 4 lớp kháng khuẩn (50 cái)",
				DiemCan = 2800,
				TriGia = 40000,
				SoLuongTon = 40
			},

			new QuaTang
			{
				MaQua = "Q12",
				TenSanPham = "Nhiệt kế điện tử kẹp nách Microlife MT200",
				DiemCan = 4200,
				TriGia = 60000,
				SoLuongTon = 15
			},

			new QuaTang
			{
				MaQua = "Q13",
				TenSanPham = "Bình giữ nhiệt Inox Long Châu 500ml",
				DiemCan = 7000,
				TriGia = 100000,
				SoLuongTon = 10
			},

			new QuaTang
			{
				MaQua = "Q14",
				TenSanPham = "Máy đo huyết áp bắp tay Omron HEM-7121",
				DiemCan = 55000,
				TriGia = 800000,
				SoLuongTon = 5
			}
		};
	}
}