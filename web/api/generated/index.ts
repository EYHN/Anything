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
  Json: any;
  Long: any;
  Url: string;
};

type IFile_Directory_Fragment = {
  readonly __typename: 'Directory';
  readonly name: string;
  readonly url: string;
  readonly mime?: Maybe<string>;
  readonly icon: string;
  readonly thumbnail?: Maybe<string>;
};

type IFile_RegularFile_Fragment = {
  readonly __typename: 'RegularFile';
  readonly name: string;
  readonly url: string;
  readonly mime?: Maybe<string>;
  readonly icon: string;
  readonly thumbnail?: Maybe<string>;
};

type IFile_UnknownFile_Fragment = {
  readonly __typename: 'UnknownFile';
  readonly name: string;
  readonly url: string;
  readonly mime?: Maybe<string>;
  readonly icon: string;
  readonly thumbnail?: Maybe<string>;
};

export type IFileFragment = IFile_Directory_Fragment | IFile_RegularFile_Fragment | IFile_UnknownFile_Fragment;

type IFileInfo_Directory_Fragment = {
  readonly __typename: 'Directory';
  readonly url: string;
  readonly name: string;
  readonly icon: string;
  readonly mime?: Maybe<string>;
  readonly thumbnail?: Maybe<string>;
  readonly metadata?: Maybe<any>;
  readonly stats: {
    readonly __typename?: 'FileStats';
    readonly creationTime?: Maybe<string>;
    readonly lastWriteTime?: Maybe<string>;
    readonly size?: Maybe<any>;
  };
};

type IFileInfo_RegularFile_Fragment = {
  readonly __typename: 'RegularFile';
  readonly url: string;
  readonly name: string;
  readonly icon: string;
  readonly mime?: Maybe<string>;
  readonly thumbnail?: Maybe<string>;
  readonly metadata?: Maybe<any>;
  readonly stats: {
    readonly __typename?: 'FileStats';
    readonly creationTime?: Maybe<string>;
    readonly lastWriteTime?: Maybe<string>;
    readonly size?: Maybe<any>;
  };
};

type IFileInfo_UnknownFile_Fragment = {
  readonly __typename: 'UnknownFile';
  readonly url: string;
  readonly name: string;
  readonly icon: string;
  readonly mime?: Maybe<string>;
  readonly thumbnail?: Maybe<string>;
  readonly metadata?: Maybe<any>;
  readonly stats: {
    readonly __typename?: 'FileStats';
    readonly creationTime?: Maybe<string>;
    readonly lastWriteTime?: Maybe<string>;
    readonly size?: Maybe<any>;
  };
};

export type IFileInfoFragment = IFileInfo_Directory_Fragment | IFileInfo_RegularFile_Fragment | IFileInfo_UnknownFile_Fragment;

export type IFileInfoQueryVariables = Exact<{
  url: Scalars['Url'];
}>;

export type IFileInfoQuery = {
  readonly __typename?: 'Query';
  readonly file:
    | {
        readonly __typename: 'Directory';
        readonly url: string;
        readonly name: string;
        readonly icon: string;
        readonly mime?: Maybe<string>;
        readonly thumbnail?: Maybe<string>;
        readonly metadata?: Maybe<any>;
        readonly stats: {
          readonly __typename?: 'FileStats';
          readonly creationTime?: Maybe<string>;
          readonly lastWriteTime?: Maybe<string>;
          readonly size?: Maybe<any>;
        };
      }
    | {
        readonly __typename: 'RegularFile';
        readonly url: string;
        readonly name: string;
        readonly icon: string;
        readonly mime?: Maybe<string>;
        readonly thumbnail?: Maybe<string>;
        readonly metadata?: Maybe<any>;
        readonly stats: {
          readonly __typename?: 'FileStats';
          readonly creationTime?: Maybe<string>;
          readonly lastWriteTime?: Maybe<string>;
          readonly size?: Maybe<any>;
        };
      }
    | {
        readonly __typename: 'UnknownFile';
        readonly url: string;
        readonly name: string;
        readonly icon: string;
        readonly mime?: Maybe<string>;
        readonly thumbnail?: Maybe<string>;
        readonly metadata?: Maybe<any>;
        readonly stats: {
          readonly __typename?: 'FileStats';
          readonly creationTime?: Maybe<string>;
          readonly lastWriteTime?: Maybe<string>;
          readonly size?: Maybe<any>;
        };
      };
};

export type IListFilesQueryVariables = Exact<{
  url: Scalars['Url'];
}>;

export type IListFilesQuery = {
  readonly __typename?: 'Query';
  readonly directory: {
    readonly __typename?: 'Directory';
    readonly entries: ReadonlyArray<
      | {
          readonly __typename: 'Directory';
          readonly name: string;
          readonly url: string;
          readonly mime?: Maybe<string>;
          readonly icon: string;
          readonly thumbnail?: Maybe<string>;
        }
      | {
          readonly __typename: 'RegularFile';
          readonly name: string;
          readonly url: string;
          readonly mime?: Maybe<string>;
          readonly icon: string;
          readonly thumbnail?: Maybe<string>;
        }
      | {
          readonly __typename: 'UnknownFile';
          readonly name: string;
          readonly url: string;
          readonly mime?: Maybe<string>;
          readonly icon: string;
          readonly thumbnail?: Maybe<string>;
        }
    >;
  };
};

export const FileFragmentDoc = gql`
  fragment File on File {
    __typename
    name
    url
    mime
    icon
    thumbnail
  }
`;
export const FileInfoFragmentDoc = gql`
  fragment FileInfo on File {
    __typename
    url
    name
    icon
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
export const FileInfoDocument = gql`
  query fileInfo($url: Url!) {
    file(url: $url) {
      ...FileInfo
    }
  }
  ${FileInfoFragmentDoc}
`;

/**
 * __useFileInfoQuery__
 *
 * To run a query within a React component, call `useFileInfoQuery` and pass it any options that fit your needs.
 * When your component renders, `useFileInfoQuery` returns an object from Apollo Client that contains loading, error, and data properties
 * you can use to render your UI.
 *
 * @param baseOptions options that will be passed into the query, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options;
 *
 * @example
 * const { data, loading, error } = useFileInfoQuery({
 *   variables: {
 *      url: // value for 'url'
 *   },
 * });
 */
export function useFileInfoQuery(baseOptions: Apollo.QueryHookOptions<IFileInfoQuery, IFileInfoQueryVariables>) {
  const options = { ...defaultOptions, ...baseOptions };
  return Apollo.useQuery<IFileInfoQuery, IFileInfoQueryVariables>(FileInfoDocument, options);
}
export function useFileInfoLazyQuery(baseOptions?: Apollo.LazyQueryHookOptions<IFileInfoQuery, IFileInfoQueryVariables>) {
  const options = { ...defaultOptions, ...baseOptions };
  return Apollo.useLazyQuery<IFileInfoQuery, IFileInfoQueryVariables>(FileInfoDocument, options);
}
export type FileInfoQueryHookResult = ReturnType<typeof useFileInfoQuery>;
export type FileInfoLazyQueryHookResult = ReturnType<typeof useFileInfoLazyQuery>;
export type FileInfoQueryResult = Apollo.QueryResult<IFileInfoQuery, IFileInfoQueryVariables>;
export const ListFilesDocument = gql`
  query listFiles($url: Url!) {
    directory(url: $url) {
      entries {
        ...File
      }
    }
  }
  ${FileFragmentDoc}
`;

/**
 * __useListFilesQuery__
 *
 * To run a query within a React component, call `useListFilesQuery` and pass it any options that fit your needs.
 * When your component renders, `useListFilesQuery` returns an object from Apollo Client that contains loading, error, and data properties
 * you can use to render your UI.
 *
 * @param baseOptions options that will be passed into the query, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options;
 *
 * @example
 * const { data, loading, error } = useListFilesQuery({
 *   variables: {
 *      url: // value for 'url'
 *   },
 * });
 */
export function useListFilesQuery(baseOptions: Apollo.QueryHookOptions<IListFilesQuery, IListFilesQueryVariables>) {
  const options = { ...defaultOptions, ...baseOptions };
  return Apollo.useQuery<IListFilesQuery, IListFilesQueryVariables>(ListFilesDocument, options);
}
export function useListFilesLazyQuery(baseOptions?: Apollo.LazyQueryHookOptions<IListFilesQuery, IListFilesQueryVariables>) {
  const options = { ...defaultOptions, ...baseOptions };
  return Apollo.useLazyQuery<IListFilesQuery, IListFilesQueryVariables>(ListFilesDocument, options);
}
export type ListFilesQueryHookResult = ReturnType<typeof useListFilesQuery>;
export type ListFilesLazyQueryHookResult = ReturnType<typeof useListFilesLazyQuery>;
export type ListFilesQueryResult = Apollo.QueryResult<IListFilesQuery, IListFilesQueryVariables>;

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
