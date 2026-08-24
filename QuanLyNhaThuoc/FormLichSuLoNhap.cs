using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QuanLyNhaThuoc
{
	public class FormLichSuLoNhap : Form
	{
		private DataGridView dgvLoNhap;

		private TextBox txtTimKiem;

		private Label lblTongLo;

		private Label lblTongTienNhap;


		public FormLichSuLoNhap(
			string maThuocMacDinh = "")
		{
			InitUI();

			if (!string.IsNullOrWhiteSpace(
				maThuocMacDinh))
			{
				txtTimKiem.Text =
					maThuocMacDinh;
			}

			LoadDanhSach();
		}


		private void InitUI()
		{
			this.Text =
				"📋 LỊCH SỬ LÔ NHẬP KHO";

			this.Size =
				new Size(1100, 620);

			this.StartPosition =
				FormStartPosition.CenterParent;

			this.BackColor =
				Color.FromArgb(245, 247, 250);

			this.Font =
				new Font(
					"Segoe UI",
					9.5F
				);


			// ====================================================
			// HEADER
			// ====================================================
			Panel pnlHeader =
				new Panel
				{
					Dock =
						DockStyle.Top,

					Height =
						70,

					Padding =
						new Padding(15)
				};


			Label lblTitle =
				new Label
				{
					Text =
						"📋 LỊCH SỬ CÁC LÔ THUỐC ĐÃ NHẬP",

					Location =
						new Point(15, 12),

					AutoSize =
						true,

					Font =
						new Font(
							"Segoe UI",
							13F,
							FontStyle.Bold
						),

					ForeColor =
						Color.FromArgb(
							24,
							43,
							73
						)
				};


			pnlHeader.Controls.Add(
				lblTitle
			);


			// ====================================================
			// THANH TÌM KIẾM
			// ====================================================
			Panel pnlSearch =
				new Panel
				{
					Dock =
						DockStyle.Top,

					Height =
						55,

					Padding =
						new Padding(
							15,
							8,
							15,
							8
						)
				};


			Label lblTim =
				new Label
				{
					Text =
						"Tìm kiếm:",

					Location =
						new Point(
							15,
							15
						),

					AutoSize =
						true,

					Font =
						new Font(
							"Segoe UI",
							9.5F,
							FontStyle.Bold
						)
				};


			txtTimKiem =
				new TextBox
				{
					Location =
						new Point(
							90,
							11
						),

					Width =
						330,

					Font =
						new Font(
							"Segoe UI",
							10F
						)
				};


			txtTimKiem.TextChanged +=
				(s, e) =>
				LoadDanhSach();


			Button btnXoa =
				new Button
				{
					Text =
						"✖ Xóa Tìm Kiếm",

					Location =
						new Point(
							430,
							9
						),

					Width =
						135,

					Height =
						31,

					BackColor =
						Color.FromArgb(
							108,
							117,
							125
						),

					ForeColor =
						Color.White,

					FlatStyle =
						FlatStyle.Flat,

					Font =
						new Font(
							"Segoe UI",
							9F,
							FontStyle.Bold
						)
				};


			btnXoa.FlatAppearance.BorderSize =
				0;


			btnXoa.Click +=
				(s, e) =>
				{
					txtTimKiem.Clear();

					txtTimKiem.Focus();
				};


			lblTongLo =
				new Label
				{
					Location =
						new Point(
							600,
							13
						),

					AutoSize =
						true,

					Font =
						new Font(
							"Segoe UI",
							9.5F,
							FontStyle.Bold
						),

					ForeColor =
						Color.FromArgb(
							0,
							122,
							204
						)
				};


			lblTongTienNhap =
				new Label
				{
					Location =
						new Point(
							760,
							13
						),

					AutoSize =
						true,

					Font =
						new Font(
							"Segoe UI",
							9.5F,
							FontStyle.Bold
						),

					ForeColor =
						Color.DarkGreen
				};


			pnlSearch.Controls.AddRange(
				new Control[]
				{
					lblTim,
					txtTimKiem,
					btnXoa,
					lblTongLo,
					lblTongTienNhap
				}
			);


			// ====================================================
			// GRID
			// ====================================================
			dgvLoNhap =
				new DataGridView
				{
					Dock =
						DockStyle.Fill,

					ReadOnly =
						true,

					AllowUserToAddRows =
						false,

					AllowUserToDeleteRows =
						false,

					AutoSizeColumnsMode =
						DataGridViewAutoSizeColumnsMode.Fill,

					BackgroundColor =
						Color.White,

					RowHeadersVisible =
						false,

					SelectionMode =
						DataGridViewSelectionMode.FullRowSelect
				};


			dgvLoNhap.EnableHeadersVisualStyles =
				false;


			dgvLoNhap.ColumnHeadersDefaultCellStyle.BackColor =
				Color.FromArgb(
					24,
					43,
					73
				);


			dgvLoNhap.ColumnHeadersDefaultCellStyle.ForeColor =
				Color.White;


			dgvLoNhap.ColumnHeadersDefaultCellStyle.Font =
				new Font(
					"Segoe UI",
					9F,
					FontStyle.Bold
				);


			dgvLoNhap.AlternatingRowsDefaultCellStyle.BackColor =
				Color.FromArgb(
					245,
					247,
					250
				);


			dgvLoNhap.CellFormatting +=
				DgvLoNhap_CellFormatting;


			// ====================================================
			// FOOTER
			// ====================================================
			Panel pnlBottom =
				new Panel
				{
					Dock =
						DockStyle.Bottom,

					Height =
						50,

					Padding =
						new Padding(10)
				};


			Button btnDong =
				new Button
				{
					Text =
						"✖ Đóng",

					Dock =
						DockStyle.Right,

					Width =
						100,

					BackColor =
						Color.FromArgb(
							108,
							117,
							125
						),

					ForeColor =
						Color.White,

					FlatStyle =
						FlatStyle.Flat,

					Font =
						new Font(
							"Segoe UI",
							9F,
							FontStyle.Bold
						)
				};


			btnDong.FlatAppearance.BorderSize =
				0;


			btnDong.Click +=
				(s, e) =>
				this.Close();


			pnlBottom.Controls.Add(
				btnDong
			);


			this.Controls.Add(
				dgvLoNhap
			);

			this.Controls.Add(
				pnlBottom
			);

			this.Controls.Add(
				pnlSearch
			);

			this.Controls.Add(
				pnlHeader
			);
		}


		private void LoadDanhSach()
		{
			string kw =
				txtTimKiem?
				.Text?
				.Trim()
				.ToLower()
				?? "";


			var ds =
				QuanLyLoNhapKhoData
				.DanhSachLoNhap
				.Where(lo =>
					lo != null &&
					(
						string.IsNullOrEmpty(kw)

						||

						(
							lo.MaLo != null &&
							lo.MaLo
								.ToLower()
								.Contains(kw)
						)

						||

						(
							lo.MaThuoc != null &&
							lo.MaThuoc
								.ToLower()
								.Contains(kw)
						)

						||

						(
							lo.TenThuoc != null &&
							lo.TenThuoc
								.ToLower()
								.Contains(kw)
						)

						||

						(
							lo.NhaCungCap != null &&
							lo.NhaCungCap
								.ToLower()
								.Contains(kw)
						)
					)
				)
				.OrderByDescending(
					lo => lo.NgayNhap
				)
				.Select(lo => new
				{
					Số_Lô =
						lo.MaLo,

					Mã_Thuốc =
						lo.MaThuoc,

					Tên_Thuốc =
						lo.TenThuoc,

					Nhà_Cung_Cấp =
						lo.NhaCungCap,

					Ngày_Nhập =
						lo.NgayNhap
							.ToString(
								"dd/MM/yyyy HH:mm"
							),

					HSD =
						lo.HanSuDung
							.ToString(
								"dd/MM/yyyy"
							),

					Số_Lượng =
						$"{lo.SoLuongNhap:N0} " +
						lo.DonViTinh,

					Giá_Nhập =
						lo.DonGiaNhap
							.ToString("N0")
						+ " VNĐ",

					Thành_Tiền =
						lo.ThanhTien
							.ToString("N0")
						+ " VNĐ",

					Trạng_Thái =
						lo.TrangThaiHanSuDung
				})
				.ToList();


			dgvLoNhap.DataSource =
				null;

			dgvLoNhap.DataSource =
				ds;


			lblTongLo.Text =
				$"Tổng: {ds.Count:N0} lô";


			decimal tongTien =
				QuanLyLoNhapKhoData
				.DanhSachLoNhap
				.Where(lo =>
					lo != null &&
					(
						string.IsNullOrEmpty(kw)
						||
						(lo.MaLo ?? "")
							.ToLower()
							.Contains(kw)
						||
						(lo.MaThuoc ?? "")
							.ToLower()
							.Contains(kw)
						||
						(lo.TenThuoc ?? "")
							.ToLower()
							.Contains(kw)
						||
						(lo.NhaCungCap ?? "")
							.ToLower()
							.Contains(kw)
					)
				)
				.Sum(lo =>
					lo.ThanhTien
				);


			lblTongTienNhap.Text =
				$"Tổng nhập: " +
				$"{tongTien:N0} VNĐ";
		}


		private void DgvLoNhap_CellFormatting(
			object sender,
			DataGridViewCellFormattingEventArgs e)
		{
			if (e.RowIndex < 0)
				return;


			DataGridViewRow row =
				dgvLoNhap.Rows[e.RowIndex];


			string trangThai =
				row.Cells["Trạng_Thái"]
					.Value?
					.ToString()
				?? "";


			if (trangThai.Contains(
				"Đã hết hạn"))
			{
				row.DefaultCellStyle.BackColor =
					Color.FromArgb(
						255,
						225,
						225
					);

				row.DefaultCellStyle.ForeColor =
					Color.DarkRed;
			}
			else if (trangThai.Contains(
				"⚠"))
			{
				row.DefaultCellStyle.BackColor =
					Color.FromArgb(
						255,
						243,
						205
					);
			}
		}
	}
}