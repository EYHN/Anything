import ReactDOM from 'react-dom';

const Helmet: React.FC = ({ children }) => {
  return ReactDOM.createPortal(children, document.head);
};

export default Helmet;
