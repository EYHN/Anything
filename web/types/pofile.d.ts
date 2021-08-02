declare module '*.po' {
  import { Messages } from '@lingui/core';
  const messages: Messages;
  export { messages };
}
