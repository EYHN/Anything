import { ILocaleMessage } from "locale/interface";
import metadataMessages from "./metadata";
import mimetypeMessages from "./mimetype";

const messages: ILocaleMessage = {
  "UI.Metadata.ShowMore": "更多",
  "UI.Metadata.ShowLess": "更少",
  ...metadataMessages,
  ...mimetypeMessages
}

export default messages;