import styled from '@emotion/styled';

interface Props {
  className?: string;
}

const Header = styled.h4(({ theme }) => ({ color: theme.colors.gray300, fontSize: '13px', fontWeight: 400 }));

const NavBarGroupHeader: React.FC<Props> = ({ className, children }) => <Header className={className}>{children}</Header>;

export default NavBarGroupHeader;
