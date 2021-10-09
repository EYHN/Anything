/* eslint-disable */
import { gql } from '@apollo/client';
import * as Apollo from '@apollo/client';
export type Maybe<T> = T | null;
export type Exact<T extends { [key: string]: unknown }> = { [K in keyof T]: T[K] };
export type MakeOptional<T, K extends keyof T> = Omit<T, K> & { [SubKey in K]?: Maybe<T[SubKey]> };
export type MakeMaybe<T, K extends keyof T> = Omit<T, K> & { [SubKey in K]: Maybe<T[SubKey]> };
const defaultOptions = {};
/** All built-in and custom scalars, mapped to their actual values */
export type Scalars = {
  ID: string;
  String: string;
  Boolean: boolean;
  Int: number;
  Float: number;
  DateTimeOffset: string;
  FileHandle: { identifier: string };
  Json: any;
  Long: any;
  Url: string;
};

type IFile_Directory_Fragment = {
  readonly __typename: 'Directory';
  readonly mime?: Maybe<string>;
  readonly icon: string;
  readonly thumbnail?: Maybe<string>;
  readonly fileHandle: { readonly __typename?: 'FileHandleRef'; readonly value: { identifier: string } };
  readonly stats: {
    readonly __typename?: 'FileStats';
    readonly creationTime?: Maybe<string>;
    readonly lastWriteTime?: Maybe<string>;
    readonly size?: Maybe<any>;
  };
};

type IFile_RegularFile_Fragment = {
  readonly __typename: 'RegularFile';
  readonly mime?: Maybe<string>;
  readonly icon: string;
  readonly thumbnail?: Maybe<string>;
  readonly fileHandle: { readonly __typename?: 'FileHandleRef'; readonly value: { identifier: string } };
  readonly stats: {
    readonly __typename?: 'FileStats';
    readonly creationTime?: Maybe<string>;
    readonly lastWriteTime?: Maybe<string>;
    readonly size?: Maybe<any>;
  };
};

type IFile_UnknownFile_Fragment = {
  readonly __typename: 'UnknownFile';
  readonly mime?: Maybe<string>;
  readonly icon: string;
  readonly thumbnail?: Maybe<string>;
  readonly fileHandle: { readonly __typename?: 'FileHandleRef'; readonly value: { identifier: string } };
  readonly stats: {
    readonly __typename?: 'FileStats';
    readonly creationTime?: Maybe<string>;
    readonly lastWriteTime?: Maybe<string>;
    readonly size?: Maybe<any>;
  };
};

export type IFileFragment = IFile_Directory_Fragment | IFile_RegularFile_Fragment | IFile_UnknownFile_Fragment;

type IFileInfo_Directory_Fragment = {
  readonly __typename: 'Directory';
  readonly name: string;
  readonly mime?: Maybe<string>;
  readonly icon: string;
  readonly thumbnail?: Maybe<string>;
  readonly metadata?: Maybe<any>;
  readonly fileHandle: { readonly __typename?: 'FileHandleRef'; readonly value: { identifier: string } };
  readonly stats: {
    readonly __typename?: 'FileStats';
    readonly creationTime?: Maybe<string>;
    readonly lastWriteTime?: Maybe<string>;
    readonly size?: Maybe<any>;
  };
};

type IFileInfo_RegularFile_Fragment = {
  readonly __typename: 'RegularFile';
  readonly name: string;
  readonly mime?: Maybe<string>;
  readonly icon: string;
  readonly thumbnail?: Maybe<string>;
  readonly metadata?: Maybe<any>;
  readonly fileHandle: { readonly __typename?: 'FileHandleRef'; readonly value: { identifier: string } };
  readonly stats: {
    readonly __typename?: 'FileStats';
    readonly creationTime?: Maybe<string>;
    readonly lastWriteTime?: Maybe<string>;
    readonly size?: Maybe<any>;
  };
};

type IFileInfo_UnknownFile_Fragment = {
  readonly __typename: 'UnknownFile';
  readonly name: string;
  readonly mime?: Maybe<string>;
  readonly icon: string;
  readonly thumbnail?: Maybe<string>;
  readonly metadata?: Maybe<any>;
  readonly fileHandle: { readonly __typename?: 'FileHandleRef'; readonly value: { identifier: string } };
  readonly stats: {
    readonly __typename?: 'FileStats';
    readonly creationTime?: Maybe<string>;
    readonly lastWriteTime?: Maybe<string>;
    readonly size?: Maybe<any>;
  };
};

export type IFileInfoFragment = IFileInfo_Directory_Fragment | IFileInfo_RegularFile_Fragment | IFileInfo_UnknownFile_Fragment;

export type IDirentFragment = {
  readonly __typename?: 'Dirent';
  readonly name: string;
  readonly file:
    | {
        readonly __typename: 'Directory';
        readonly mime?: Maybe<string>;
        readonly icon: string;
        readonly thumbnail?: Maybe<string>;
        readonly fileHandle: { readonly __typename?: 'FileHandleRef'; readonly value: { identifier: string } };
        readonly stats: {
          readonly __typename?: 'FileStats';
          readonly creationTime?: Maybe<string>;
          readonly lastWriteTime?: Maybe<string>;
          readonly size?: Maybe<any>;
        };
      }
    | {
        readonly __typename: 'RegularFile';
        readonly mime?: Maybe<string>;
        readonly icon: string;
        readonly thumbnail?: Maybe<string>;
        readonly fileHandle: { readonly __typename?: 'FileHandleRef'; readonly value: { identifier: string } };
        readonly stats: {
          readonly __typename?: 'FileStats';
          readonly creationTime?: Maybe<string>;
          readonly lastWriteTime?: Maybe<string>;
          readonly size?: Maybe<any>;
        };
      }
    | {
        readonly __typename: 'UnknownFile';
        readonly mime?: Maybe<string>;
        readonly icon: string;
        readonly thumbnail?: Maybe<string>;
        readonly fileHandle: { readonly __typename?: 'FileHandleRef'; readonly value: { identifier: string } };
        readonly stats: {
          readonly __typename?: 'FileStats';
          readonly creationTime?: Maybe<string>;
          readonly lastWriteTime?: Maybe<string>;
          readonly size?: Maybe<any>;
        };
      };
};

export type IFileInfoByFileHandleQueryVariables = Exact<{
  fileHandle: Scalars['FileHandle'];
}>;

export type IFileInfoByFileHandleQuery = {
  readonly __typename?: 'Query';
  readonly openFileHandle: {
    readonly __typename?: 'FileHandleRef';
    readonly openFile:
      | {
          readonly __typename: 'Directory';
          readonly name: string;
          readonly mime?: Maybe<string>;
          readonly icon: string;
          readonly thumbnail?: Maybe<string>;
          readonly metadata?: Maybe<any>;
          readonly fileHandle: { readonly __typename?: 'FileHandleRef'; readonly value: { identifier: string } };
          readonly stats: {
            readonly __typename?: 'FileStats';
            readonly creationTime?: Maybe<string>;
            readonly lastWriteTime?: Maybe<string>;
            readonly size?: Maybe<any>;
          };
        }
      | {
          readonly __typename: 'RegularFile';
          readonly name: string;
          readonly mime?: Maybe<string>;
          readonly icon: string;
          readonly thumbnail?: Maybe<string>;
          readonly metadata?: Maybe<any>;
          readonly fileHandle: { readonly __typename?: 'FileHandleRef'; readonly value: { identifier: string } };
          readonly stats: {
            readonly __typename?: 'FileStats';
            readonly creationTime?: Maybe<string>;
            readonly lastWriteTime?: Maybe<string>;
            readonly size?: Maybe<any>;
          };
        }
      | {
          readonly __typename: 'UnknownFile';
          readonly name: string;
          readonly mime?: Maybe<string>;
          readonly icon: string;
          readonly thumbnail?: Maybe<string>;
          readonly metadata?: Maybe<any>;
          readonly fileHandle: { readonly __typename?: 'FileHandleRef'; readonly value: { identifier: string } };
          readonly stats: {
            readonly __typename?: 'FileStats';
            readonly creationTime?: Maybe<string>;
            readonly lastWriteTime?: Maybe<string>;
            readonly size?: Maybe<any>;
          };
        };
  };
};

export type IListFilesByUrlQueryVariables = Exact<{
  url: Scalars['Url'];
}>;

export type IListFilesByUrlQuery = {
  readonly __typename?: 'Query';
  readonly createFileHandle: {
    readonly __typename?: 'FileHandleRef';
    readonly openDirectory: {
      readonly __typename?: 'Directory';
      readonly entries: ReadonlyArray<{
        readonly __typename?: 'Dirent';
        readonly name: string;
        readonly file:
          | {
              readonly __typename: 'Directory';
              readonly mime?: Maybe<string>;
              readonly icon: string;
              readonly thumbnail?: Maybe<string>;
              readonly fileHandle: { readonly __typename?: 'FileHandleRef'; readonly value: { identifier: string } };
              readonly stats: {
                readonly __typename?: 'FileStats';
                readonly creationTime?: Maybe<string>;
                readonly lastWriteTime?: Maybe<string>;
                readonly size?: Maybe<any>;
              };
            }
          | {
              readonly __typename: 'RegularFile';
              readonly mime?: Maybe<string>;
              readonly icon: string;
              readonly thumbnail?: Maybe<string>;
              readonly fileHandle: { readonly __typename?: 'FileHandleRef'; readonly value: { identifier: string } };
              readonly stats: {
                readonly __typename?: 'FileStats';
                readonly creationTime?: Maybe<string>;
                readonly lastWriteTime?: Maybe<string>;
                readonly size?: Maybe<any>;
              };
            }
          | {
              readonly __typename: 'UnknownFile';
              readonly mime?: Maybe<string>;
              readonly icon: string;
              readonly thumbnail?: Maybe<string>;
              readonly fileHandle: { readonly __typename?: 'FileHandleRef'; readonly value: { identifier: string } };
              readonly stats: {
                readonly __typename?: 'FileStats';
                readonly creationTime?: Maybe<string>;
                readonly lastWriteTime?: Maybe<string>;
                readonly size?: Maybe<any>;
              };
            };
      }>;
    };
  };
};

export const FileInfoFragmentDoc = gql`
  fragment FileInfo on File {
    __typename
    fileHandle {
      value
    }
    name
    mime
    icon
    thumbnail
    metadata
    stats {
      creationTime
      lastWriteTime
      size
    }
  }
`;
export const FileFragmentDoc = gql`
  fragment File on File {
    __typename
    fileHandle {
      value
    }
    mime
    icon
    thumbnail
    stats {
      creationTime
      lastWriteTime
      size
    }
  }
`;
export const DirentFragmentDoc = gql`
  fragment Dirent on Dirent {
    name
    file {
      ...File
    }
  }
  ${FileFragmentDoc}
`;
export const FileInfoByFileHandleDocument = gql`
  query fileInfoByFileHandle($fileHandle: FileHandle!) {
    openFileHandle(fileHandle: $fileHandle) {
      openFile {
        ...FileInfo
      }
    }
  }
  ${FileInfoFragmentDoc}
`;

/**
 * __useFileInfoByFileHandleQuery__
 *
 * To run a query within a React component, call `useFileInfoByFileHandleQuery` and pass it any options that fit your needs.
 * When your component renders, `useFileInfoByFileHandleQuery` returns an object from Apollo Client that contains loading, error, and data properties
 * you can use to render your UI.
 *
 * @param baseOptions options that will be passed into the query, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options;
 *
 * @example
 * const { data, loading, error } = useFileInfoByFileHandleQuery({
 *   variables: {
 *      fileHandle: // value for 'fileHandle'
 *   },
 * });
 */
export function useFileInfoByFileHandleQuery(
  baseOptions: Apollo.QueryHookOptions<IFileInfoByFileHandleQuery, IFileInfoByFileHandleQueryVariables>,
) {
  const options = { ...defaultOptions, ...baseOptions };
  return Apollo.useQuery<IFileInfoByFileHandleQuery, IFileInfoByFileHandleQueryVariables>(FileInfoByFileHandleDocument, options);
}
export function useFileInfoByFileHandleLazyQuery(
  baseOptions?: Apollo.LazyQueryHookOptions<IFileInfoByFileHandleQuery, IFileInfoByFileHandleQueryVariables>,
) {
  const options = { ...defaultOptions, ...baseOptions };
  return Apollo.useLazyQuery<IFileInfoByFileHandleQuery, IFileInfoByFileHandleQueryVariables>(FileInfoByFileHandleDocument, options);
}
export type FileInfoByFileHandleQueryHookResult = ReturnType<typeof useFileInfoByFileHandleQuery>;
export type FileInfoByFileHandleLazyQueryHookResult = ReturnType<typeof useFileInfoByFileHandleLazyQuery>;
export type FileInfoByFileHandleQueryResult = Apollo.QueryResult<IFileInfoByFileHandleQuery, IFileInfoByFileHandleQueryVariables>;
export const ListFilesByUrlDocument = gql`
  query listFilesByUrl($url: Url!) {
    createFileHandle(url: $url) {
      openDirectory {
        entries {
          ...Dirent
        }
      }
    }
  }
  ${DirentFragmentDoc}
`;

/**
 * __useListFilesByUrlQuery__
 *
 * To run a query within a React component, call `useListFilesByUrlQuery` and pass it any options that fit your needs.
 * When your component renders, `useListFilesByUrlQuery` returns an object from Apollo Client that contains loading, error, and data properties
 * you can use to render your UI.
 *
 * @param baseOptions options that will be passed into the query, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options;
 *
 * @example
 * const { data, loading, error } = useListFilesByUrlQuery({
 *   variables: {
 *      url: // value for 'url'
 *   },
 * });
 */
export function useListFilesByUrlQuery(baseOptions: Apollo.QueryHookOptions<IListFilesByUrlQuery, IListFilesByUrlQueryVariables>) {
  const options = { ...defaultOptions, ...baseOptions };
  return Apollo.useQuery<IListFilesByUrlQuery, IListFilesByUrlQueryVariables>(ListFilesByUrlDocument, options);
}
export function useListFilesByUrlLazyQuery(baseOptions?: Apollo.LazyQueryHookOptions<IListFilesByUrlQuery, IListFilesByUrlQueryVariables>) {
  const options = { ...defaultOptions, ...baseOptions };
  return Apollo.useLazyQuery<IListFilesByUrlQuery, IListFilesByUrlQueryVariables>(ListFilesByUrlDocument, options);
}
export type ListFilesByUrlQueryHookResult = ReturnType<typeof useListFilesByUrlQuery>;
export type ListFilesByUrlLazyQueryHookResult = ReturnType<typeof useListFilesByUrlLazyQuery>;
export type ListFilesByUrlQueryResult = Apollo.QueryResult<IListFilesByUrlQuery, IListFilesByUrlQueryVariables>;

export interface PossibleTypesResultData {
  possibleTypes: {
    [key: string]: string[];
  };
}
const result: PossibleTypesResultData = {
  possibleTypes: {
    File: ['Directory', 'RegularFile', 'UnknownFile'],
  },
};
export default result;
