import styled from '@emotion/styled';

interface Props {
  className?: string;
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

const TestPreview = styled.span({
  display: 'inline-block',
  width: '174px',
  height: '96px',

  background: 'rgb(242, 107, 163)',
  borderRadius: '8px',
  marginBottom: '16px',

  boxShadow: '0px 2px 4px rgba(0, 0, 0, 0.15)',
});

const FileInfo: React.FC<Props> = ({ className }) => (
  <Container className={className}>
    <TestPreview />
    <FileName>GOOD BOOK.png</FileName>
    <FileDesc>
      <FileType>PNG Image</FileType>1.2 MB
    </FileDesc>
  </Container>
);

export default FileInfo;
