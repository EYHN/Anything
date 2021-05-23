import { ILocaleMetadataMessage } from 'locale/interface';

const metadataMessages: ILocaleMetadataMessage = {
  'Metadata.Information': '基本信息',
  'Metadata.Information.CreationTime': '创建时间',
  'Metadata.Information.CreationTime.Value': '{value, date}',
  'Metadata.Information.ModifyTime': '修改时间',
  'Metadata.Information.ModifyTime.Value': '{value, date}',
  'Metadata.Palette': '调色盘',
  'Metadata.Image': '图片',
  'Metadata.Image.Width': '宽度',
  'Metadata.Image.Width.Value': '{value} 像素',
  'Metadata.Image.Height': '高度',
  'Metadata.Image.Height.Value': '{value} 像素',
  'Metadata.Image.Channels': '通道',
  'Metadata.Image.BitDepth': '位深度',
  'Metadata.Image.PngColorType': 'PNG 颜色类型',
  'Metadata.Image.DataPrecision': '数据精度',
  'Metadata.Image.Gamma': 'Gamma',
  'Metadata.Image.SubfileType': '数据类型',
  'Metadata.Image.Orientation': '方向',
  'Metadata.Image.XResolution': 'X 分辨率',
  'Metadata.Image.YResolution': 'Y 分辨率',
  'Metadata.Image.DateTime': '日期',
  'Metadata.Image.DateTime.Value': '{value, date}',
  'Metadata.Image.ColorSpace': '色彩空间',
  'Metadata.Image.UserComment': '备注',
  'Metadata.Image.ExifVersion': 'Exif 版本',
  'Metadata.Image.PageNumber': '页码',
  'Metadata.Image.CompressionType': '压缩',
  'Metadata.Image.InterlaceMethod': '隔行扫描',
  'Metadata.Image.YCbCrPositioning': 'YCbCr 位置',
  'Metadata.Image.ComponentsConfiguration': '通道配置',
  'Metadata.Image.JpegCompressionType': 'JPEG 压缩',
  'Metadata.Image.JpegCompressionType.Value': `{value, plural, 
    =0  {Baseline}
    =1  {Extended sequential, Huffman}
    =2  {Progressive, Huffman}
    =3  {Lossless, Huffman}
    =5  {Differential sequential, Huffman}
    =6  {Differential progressive, Huffman}
    =7  {Differential lossless, Huffman}
    =8  {Reserved for JPEG extensions}
    =9  {Extended sequential, arithmetic}
    =10 {Progressive, arithmetic}
    =11 {Lossless, arithmetic}
    =13 {Differential sequential, arithmetic}
    =14 {Differential progressive, arithmetic}
    =15 {Differential lossless, arithmetic}
    other {未知 (#)}
  }`,
  'Metadata.Camera': '相机',
  'Metadata.Camera.Make': '制造商',
  'Metadata.Camera.Model': '型号',
  'Metadata.Camera.ExposureTime': '曝光时间',
  'Metadata.Camera.FNumber': '光圈',
  'Metadata.Camera.ExposureProgram': '曝光程序',
  'Metadata.Camera.ShutterSpeed': '快门速度',
  'Metadata.Camera.IsoSpeed': 'ISO 速度',
  'Metadata.Camera.Aperture': '光圈尺寸',
  'Metadata.Camera.ExposureBias': '曝光补偿',
  'Metadata.Camera.MeteringMode': '测光模式',
  'Metadata.Camera.Flash': '闪光灯',
  'Metadata.Camera.FocalLength': '焦距',
  'Metadata.Camera.DateTimeOriginal': '原始日期',
  'Metadata.Camera.DateTimeOriginal.Value': '{value, date}',
  'Metadata.Camera.DateTimeDigitized': '数字化日期',
  'Metadata.Camera.DateTimeDigitized.Value': '{value, date}',
  'Metadata.Camera.ExposureMode': '曝光模式',
  'Metadata.Camera.WhiteBalance': '白平衡',
  'Metadata.Camera.WhiteBalanceMode': '白平衡模式',
  'Metadata.Camera.SceneCaptureType': '拍摄场景类型',
  'Metadata.Camera.LensMake': '镜头制造商',
  'Metadata.Camera.LensModel': '镜头型号',
  'Metadata.Camera.FocalPlaneXResolution': '焦平面 X 分辨率',
  'Metadata.Camera.FocalPlaneYResolution': '焦平面 Y 分辨率',
  'Metadata.Camera.CustomRendered': '自定义渲染',
  'Metadata.Camera.LensSerialNumber': '镜头序列号',
  'Metadata.Camera.LensSpecification': '镜头规格',
  'Metadata.Interoperability': '互操作性',
  'Metadata.Interoperability.InteroperabilityIndex': '互操作性规则',
  'Metadata.Interoperability.InteroperabilityVersion': '互操作性版本',
};

export default metadataMessages;
