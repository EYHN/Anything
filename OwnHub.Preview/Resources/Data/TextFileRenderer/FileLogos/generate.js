const fs = require("fs");
const path = require("path");

(async () => {
    const result = {}

    const FileLogoDir = path.join(__dirname, "./");
    const LogoCategories = await fs.promises.readdir(FileLogoDir);
    for (const LogoCategory of LogoCategories) {
        try {
            const LogoFiles = await fs.promises.readdir(path.join(FileLogoDir, LogoCategory));
            for (const LogoFile of LogoFiles) {
                const LogoName = path.basename(LogoFile, ".svg");
                const LogoKey = LogoCategory + "/" + LogoName;
                const LogoSvg = await fs.promises.readFile(path.join(FileLogoDir, LogoCategory, LogoFile), {encoding: "utf-8"});
                const LogoPath = LogoSvg.match(/(?<=(<path.*d="))((?!").)*/g)[0];
                const LogoFill = LogoSvg.match(/(?<=(<path.*fill="))((?!").)*/g)[0] || "#7f7f7f";
                result[LogoKey] = {path: LogoPath, fill: LogoFill};
                console.log("using " + LogoKey);
            }
        }
        catch(err) {

        }
    }

    await fs.promises.writeFile(path.join(FileLogoDir, "FileLogos.json"), JSON.stringify(result, null, 2));
})();