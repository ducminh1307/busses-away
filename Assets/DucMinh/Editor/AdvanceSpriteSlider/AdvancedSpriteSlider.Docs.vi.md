# AdvancedSpriteSlider

`AdvancedSpriteSlider` là công cụ Editor dùng để cắt sprite từ `Texture2D` ngay trong Unity. Công cụ hỗ trợ cắt thủ công, cắt tự động theo vùng có alpha, cắt theo lưới, xem trước trực quan và xuất từng sprite ra file PNG riêng.

## Mục đích

Công cụ này được tạo ra để giúp:

- Cắt nhiều sprite nhanh hơn so với thao tác thủ công trong Sprite Editor mặc định.
- Kiểm soát trực tiếp vùng cắt ngay trên preview.
- Dễ chỉnh pivot, border, tên sprite và thư mục export.
- Hỗ trợ quy trình làm việc cho texture atlas, icon sheet, UI sheet hoặc sprite sheet nhân vật.

## Vị trí mở công cụ

Trong Unity Editor, mở menu:

```text
DucMinh/Advance Sprite Slider
```

## Chức năng chính

### 1. Chọn texture

- Chọn một `Texture2D` để bắt đầu làm việc.
- Khi đổi texture, công cụ sẽ cố gắng đọc lại các slice đã có sẵn từ importer nếu texture đang ở chế độ nhiều sprite.

### 2. Chế độ tương tác

Công cụ có 2 chế độ tương tác chính:

- `Edit`: dùng để tạo, kéo, đổi kích thước và chỉnh từng slice.
- `Select`: dùng để chọn nhiều slice cùng lúc trên vùng preview.

### 3. Các chế độ cắt

#### Manual

- Người dùng tự tạo slice bằng chuột trên vùng preview.
- Phù hợp khi sprite có bố cục không đều hoặc cần kiểm soát chính xác.

#### Auto

- Hệ thống tự quét texture dựa trên vùng pixel có alpha.
- Mỗi vùng liên thông đủ lớn sẽ được tạo thành một slice riêng.
- Phù hợp với sprite sheet có các phần tử tách nhau rõ ràng.

#### Grid By Cell Size

- Cắt theo kích thước ô cố định.
- Có thể cấu hình:
  - `Cell Size`
  - `Padding`
  - `Offset`

#### Grid By Cell Count

- Cắt theo số lượng hàng và cột.
- Công cụ sẽ tự tính kích thước từng ô từ kích thước texture.

## Khu vực giao diện

### Panel bên trái

Hiển thị các thiết lập chính:

- Texture
- Interaction Mode
- Slicing Mode
- Thông số cắt
- Save Folder
- Naming Prefix
- Pivot
- Custom Pivot
- Thông tin slice đang chọn

### Panel bên phải

Hiển thị preview texture và vùng slice:

- Zoom in / zoom out
- Center view
- Tạo slice bằng chuột
- Kéo để di chuyển slice
- Kéo các handle để đổi kích thước
- Xem tên và kích thước slice đang chọn

## Thiết lập sprite

Mỗi slice có thể mang các thông tin sau:

- `Name`: tên sprite
- `Rect`: vùng cắt
- `Border`: viền 9-slice theo thứ tự trái, dưới, phải, trên
- `Pivot`: kiểu pivot
- `Custom Pivot`: giá trị pivot tùy chỉnh nếu chọn `Custom`

## Sao chép và dán dữ liệu

Công cụ hỗ trợ một số thao tác tiện lợi:

- Copy/Paste `Rect`
- Copy/Paste `Border`
- Copy/Paste slice bằng phím tắt khi đang ở chế độ chỉnh sửa

Điều này giúp tái sử dụng nhanh cấu hình giữa các sprite có cấu trúc tương tự.

## Xuất sprite

Khi bấm `Apply Slices`, công cụ sẽ:

1. Đảm bảo texture có thể đọc được.
2. Xuất từng slice thành file PNG riêng trong thư mục đã chọn.
3. Thiết lập importer của từng file PNG thành `Sprite`.
4. Áp dụng `Pivot` và `Border` tương ứng cho từng sprite đã xuất.
5. Cập nhật lại importer của texture gốc với danh sách slice hiện tại.

## Save Folder

- `Save Folder` là thư mục trong project dùng để lưu các PNG đã tách.
- Nếu không chọn gì, công cụ mặc định dùng thư mục `Assets`.
- Thông tin thư mục được lưu bằng `EditorPrefs`.

## Preset

Công cụ hỗ trợ lưu và nạp preset cắt:

- `Save Preset`: lưu thông số cắt ra file JSON
- `Load Preset`: nạp lại thông số từ file JSON

Preset hiện tập trung vào các tham số:

- `Cell Size`
- `Padding`
- `Offset`
- `Cell Count`
- `Slice Mode`

## Luồng làm việc đề xuất

Quy trình sử dụng phổ biến:

1. Chọn texture cần cắt.
2. Chọn `Slicing Mode` phù hợp.
3. Bấm `Slice` để tạo danh sách slice ban đầu.
4. Chuyển sang `Edit` để tinh chỉnh từng vùng cắt nếu cần.
5. Đặt `Naming Prefix`, `Pivot`, `Border`.
6. Chọn `Save Folder`.
7. Bấm `Apply Slices` để xuất sprite và cập nhật importer.

## Một số lưu ý

- Texture cần có dữ liệu alpha rõ ràng nếu dùng chế độ `Auto`.
- Khi export, tên sprite nên tránh trùng lặp để hạn chế ghi đè file ngoài ý muốn.
- Nếu texture quá lớn, thao tác quét pixel có thể mất thêm thời gian.
- Khi công cụ tự bật chế độ readable và bỏ nén texture, importer của texture sẽ được reimport lại.

## Cấu trúc mã nguồn hiện tại

Mã nguồn được chia thành nhiều file `partial class` để dễ bảo trì:

- `AdvanceSpriteSlider.cs`: khai báo chính và trạng thái của cửa sổ editor
- `AdvanceSpriteSlider.UI.cs`: dựng giao diện UI Toolkit
- `AdvanceSpriteSlider.Preview.cs`: xử lý preview, chọn, kéo, resize
- `AdvanceSpriteSlider.Slicing.cs`: điểm vào cho hành vi cắt chính
- `AdvanceSpriteSlider.Slicing.Helpers.cs`: helper dùng chung
- `AdvanceSpriteSlider.Slicing.Presets.cs`: preset và zoom bằng chuột
- `AdvanceSpriteSlider.Slicing.Generation.cs`: logic tạo slice
- `AdvanceSpriteSlider.Slicing.Export.cs`: export và đồng bộ importer

## Gợi ý mở rộng trong tương lai

- Hỗ trợ đổi tên theo mẫu nâng cao.
- Hỗ trợ batch process nhiều texture cùng lúc.
- Thêm xác thực tên file trùng trước khi export.
- Thêm preset cho `Pivot`, `Border` và thư mục lưu.
- Thêm chức năng undo/redo sâu hơn cho các thao tác hàng loạt.

## Tóm tắt

`AdvancedSpriteSlider` là một công cụ cắt sprite linh hoạt, phù hợp cho cả nhu cầu cắt nhanh theo lưới lẫn tinh chỉnh thủ công trực tiếp trong Editor. Việc tách mã nguồn thành nhiều `partial class` giúp công cụ dễ đọc, dễ mở rộng và thuận tiện hơn cho quá trình bảo trì lâu dài.
