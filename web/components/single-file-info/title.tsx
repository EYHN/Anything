import styled from '@emotion/styled';
import { IFileInfoFragment } from 'api';
import FileThumbnail from 'components/file-icons';
import { useI18n } from 'i18n';
import prettyFileSize from 'utils/filesize';

interface Props {
  className?: string;
  file: IFileInfoFragment;
}

const Container = styled.div({
  textAlign: 'center',
});

const FileName = styled.p({
  fontSize: '15px',
  lineHeight: 1.4,
  fontWeight: 400,
  margin: '0 0 8px',
});

const FileDesc = styled.p(({ theme }) => ({
  fontSize: '12px',
  lineHeight: 1.4,
  fontWeight: 400,
  color: theme.colors.gray200,
  margin: 0,
}));

const FileType = styled.span({
  marginRight: '10px',
});

const Title: React.FC<Props> = ({ className, file }) => {
  const { name, mime, stats } = file;
  const { size } = stats;
  const { localeMimetype } = useI18n();
  return (
    <Container className={className}>
      <FileThumbnail file={file} width={174} height={174} dropShadow />
      <FileName>{name}</FileName>
      <FileDesc>
        <FileType>{localeMimetype(mime ?? undefined)}</FileType>
        {typeof size === 'number' && prettyFileSize(size)}
      </FileDesc>
    </Container>
  );
};

export default Title;
