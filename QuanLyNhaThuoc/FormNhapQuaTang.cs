using System;
using System.Drawing;
using System.Windows.Forms;

namespace QuanLyNhaThuoc
{
	public class FormNhapQuaTang : Form
	{
		private QuaTang _qua;

		private ComboBox cboNguonCap;

		private NumericUpDown numSoLuong;

		private DateTimePicker dtpHsd;

		private TextBox txtGhiChu;


		public FormNhapQuaTang(QuaTang qua)
		{
			_qua = qua;

			InitUI();
		}


		private void InitUI()
		{
			this.Text =
				"🎁 PHIẾU NHẬP QUÀ TẶNG";

			this.Size =
				new Size(540, 430);

			this.StartPosition =
				FormStartPosition.CenterParent;

			this.FormBorderStyle =
				FormBorderStyle.FixedDialog;

			this.MaximizeBox = false;
			this.MinimizeBox = false;

			this.BackColor =
				Color.FromArgb(245, 247, 250);

			this.Font =
				new Font("Segoe UI", 9.5F);


			TableLayoutPanel pnl =
				new TableLayoutPanel
				{
					Dock =
						DockStyle.Fill,

					Padding =
						new Padding(20),

					ColumnCount =
						2,

					RowCount =
						8
				};


			pnl.ColumnStyles.Add(
				new ColumnStyle(
					SizeType.Absolute,
					145
				)
			);

			pnl.ColumnStyles.Add(
				new ColumnStyle(
					SizeType.Percent,
					100
				)
			);


			for (int i = 0; i < 8; i++)
			{
				pnl.RowStyles.Add(
					new RowStyle(
						SizeType.Absolute,
						42
					)
				);
			}


			// ====================================================
			// MÃ QUÀ
			// ====================================================
			pnl.Controls.Add(
				new Label
				{
					Text = "Mã quà:",
					Anchor = AnchorStyles.Left,
					Font = new Font(
						"Segoe UI",
						9.5F,
						FontStyle.Bold
					)
				},
				0,
				0
			);


			pnl.Controls.Add(
				new Label
				{
					Text = _qua.MaQua,
					Anchor = AnchorStyles.Left,
					ForeColor = Color.DarkBlue,
					Font = new Font(
						"Segoe UI",
						10F,
						FontStyle.Bold
					)
				},
				1,
				0
			);


			// ====================================================
			// TÊN QUÀ
			// ====================================================
			pnl.Controls.Add(
				new Label
				{
					Text = "Tên quà:",
					Anchor = AnchorStyles.Left
				},
				0,
				1
			);


			pnl.Controls.Add(
				new Label
				{
					Text = _qua.TenSanPham,
					Anchor = AnchorStyles.Left,
					AutoSize = true,
					Font = new Font(
						"Segoe UI",
						9.5F,
						FontStyle.Bold
					)
				},
				1,
				1
			);


			// ====================================================
			// TỒN HIỆN TẠI
			// ====================================================
			pnl.Controls.Add(
				new Label
				{
					Text = "Tồn hiện tại:",
					Anchor = AnchorStyles.Left
				},
				0,
				2
			);


			pnl.Controls.Add(
				new Label
				{
					Text =
						$"{_qua.SoLuongTon:N0} quà",

					Anchor =
						AnchorStyles.Left,

					ForeColor =
						_qua.SoLuongTon <= 10
						? Color.DarkRed
						: Color.DarkGreen,

					Font =
						new Font(
							"Segoe UI",
							10F,
							FontStyle.Bold
						)
				},
				1,
				2
			);


			// ====================================================
			// NGUỒN CẤP
			// ====================================================
			pnl.Controls.Add(
				new Label
				{
					Text = "Nguồn cấp:",
					Anchor = AnchorStyles.Left,
					Font = new Font(
						"Segoe UI",
						9.5F,
						FontStyle.Bold
					)
				},
				0,
				3
			);


			cboNguonCap =
				new ComboBox
				{
					Dock =
						DockStyle.Fill,

					DropDownStyle =
						ComboBoxStyle.DropDown
				};


			cboNguonCap.Items.AddRange(
				new object[]
				{
					"Kho Marketing",
					"Nhà cung cấp tài trợ",
					"Chương trình khuyến mãi",
					"Mua bổ sung",
					"Kho tổng"
				}
			);


			cboNguonCap.SelectedIndex = 0;


			pnl.Controls.Add(
				cboNguonCap,
				1,
				3
			);


			// ====================================================
			// SỐ LƯỢNG
			// ====================================================
			pnl.Controls.Add(
				new Label
				{
					Text = "Số lượng nhập:",
					Anchor = AnchorStyles.Left,
					Font = new Font(
						"Segoe UI",
						9.5F,
						FontStyle.Bold
					)
				},
				0,
				4
			);


			numSoLuong =
				new NumericUpDown
				{
					Dock =
						DockStyle.Fill,

					Minimum =
						1,

					Maximum =
						100000,

					Value =
						10,

					ThousandsSeparator =
						true
				};


			pnl.Controls.Add(
				numSoLuong,
				1,
				4
			);


			// ====================================================
			// HSD - CÓ THỂ BỎ CHỌN
			// ====================================================
			pnl.Controls.Add(
				new Label
				{
					Text = "Hạn sử dụng:",
					Anchor = AnchorStyles.Left
				},
				0,
				5
			);


			dtpHsd =
				new DateTimePicker
				{
					Dock =
						DockStyle.Fill,

					Format =
						DateTimePickerFormat.Custom,

					CustomFormat =
						"dd/MM/yyyy",

					ShowCheckBox =
						true,

					Checked =
						false,

					Value =
						DateTime.Today.AddYears(2)
				};


			pnl.Controls.Add(
				dtpHsd,
				1,
				5
			);


			// ====================================================
			// GHI CHÚ
			// ====================================================
			pnl.Controls.Add(
				new Label
				{
					Text = "Ghi chú:",
					Anchor = AnchorStyles.Left
				},
				0,
				6
			);


			txtGhiChu =
				new TextBox
				{
					Dock =
						DockStyle.Fill,

					Text =
						"Bổ sung tồn quà tặng"
				};


			pnl.Controls.Add(
				txtGhiChu,
				1,
				6
			);


			// ====================================================
			// BUTTON
			// ====================================================
			Button btnXacNhan =
				new Button
				{
					Text =
						"✔ XÁC NHẬN NHẬP QUÀ",

					Dock =
						DockStyle.Fill,

					Height =
						36,

					BackColor =
						Color.FromArgb(
							40,
							167,
							69
						),

					ForeColor =
						Color.White,

					FlatStyle =
						FlatStyle.Flat,

					Font =
						new Font(
							"Segoe UI",
							9.5F,
							FontStyle.Bold
						),

					Cursor =
						Cursors.Hand
				};


			btnXacNhan.FlatAppearance.BorderSize =
				0;


			btnXacNhan.Click +=
				BtnXacNhan_Click;


			pnl.Controls.Add(
				btnXacNhan,
				1,
				7
			);


			this.Controls.Add(pnl);
		}


		private void BtnXacNhan_Click(
			object sender,
			EventArgs e)
		{
			int soLuong =
				(int)numSoLuong.Value;


			string nguonCap =
				cboNguonCap.Text.Trim();


			if (string.IsNullOrWhiteSpace(
				nguonCap))
			{
				MessageBox.Show(
					"Vui lòng nhập nguồn cấp quà!",
					"Thiếu thông tin",
					MessageBoxButtons.OK,
					MessageBoxIcon.Warning
				);

				return;
			}


			DateTime? hsd =
				null;


			if (dtpHsd.Checked)
			{
				if (dtpHsd.Value.Date <=
					DateTime.Today)
				{
					MessageBox.Show(
						"Hạn sử dụng phải lớn hơn ngày hiện tại!",
						"HSD không hợp lệ",
						MessageBoxButtons.OK,
						MessageBoxIcon.Warning
					);

					return;
				}


				hsd =
					dtpHsd.Value.Date;
			}


			int tonCu =
				_qua.SoLuongTon;


			bool thanhCong =
				QuanLyKhoQuaTangData
				.NhapQua(
					_qua,
					soLuong,
					nguonCap,
					hsd,
					txtGhiChu.Text.Trim()
				);


			if (!thanhCong)
			{
				MessageBox.Show(
					"Không thể nhập quà!",
					"Lỗi",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error
				);

				return;
			}


			MessageBox.Show(
				$"Nhập quà thành công!\n\n" +
				$"• Quà: {_qua.TenSanPham}\n" +
				$"• Nguồn cấp: {nguonCap}\n" +
				$"• Tồn cũ: {tonCu:N0}\n" +
				$"• Nhập thêm: +{soLuong:N0}\n" +
				$"• Tồn mới: {_qua.SoLuongTon:N0}" +
				(
					hsd.HasValue
					? $"\n• HSD: {hsd.Value:dd/MM/yyyy}"
					: "\n• HSD: Không áp dụng"
				),

				"Nhập quà thành công",

				MessageBoxButtons.OK,
				MessageBoxIcon.Information
			);


			this.DialogResult =
				DialogResult.OK;

			this.Close();
		}
	}
}