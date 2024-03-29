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
  readonly _id: string;
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
  readonly _id: string;
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
  readonly _id: string;
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
  readonly _id: string;
  readonly name: string;
  readonly mime?: Maybe<string>;
  readonly icon: string;
  readonly thumbnail?: Maybe<string>;
  readonly metadata?: Maybe<any>;
  readonly tags: ReadonlyArray<string>;
  readonly notes?: Maybe<string>;
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
  readonly _id: string;
  readonly name: string;
  readonly mime?: Maybe<string>;
  readonly icon: string;
  readonly thumbnail?: Maybe<string>;
  readonly metadata?: Maybe<any>;
  readonly tags: ReadonlyArray<string>;
  readonly notes?: Maybe<string>;
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
  readonly _id: string;
  readonly name: string;
  readonly mime?: Maybe<string>;
  readonly icon: string;
  readonly thumbnail?: Maybe<string>;
  readonly metadata?: Maybe<any>;
  readonly tags: ReadonlyArray<string>;
  readonly notes?: Maybe<string>;
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
        readonly _id: string;
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
        readonly _id: string;
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
        readonly _id: string;
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
          readonly _id: string;
          readonly name: string;
          readonly mime?: Maybe<string>;
          readonly icon: string;
          readonly thumbnail?: Maybe<string>;
          readonly metadata?: Maybe<any>;
          readonly tags: ReadonlyArray<string>;
          readonly notes?: Maybe<string>;
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
          readonly _id: string;
          readonly name: string;
          readonly mime?: Maybe<string>;
          readonly icon: string;
          readonly thumbnail?: Maybe<string>;
          readonly metadata?: Maybe<any>;
          readonly tags: ReadonlyArray<string>;
          readonly notes?: Maybe<string>;
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
          readonly _id: string;
          readonly name: string;
          readonly mime?: Maybe<string>;
          readonly icon: string;
          readonly thumbnail?: Maybe<string>;
          readonly metadata?: Maybe<any>;
          readonly tags: ReadonlyArray<string>;
          readonly notes?: Maybe<string>;
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
              readonly _id: string;
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
              readonly _id: string;
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
              readonly _id: string;
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

export type ISetTagsMutationVariables = Exact<{
  fileHandle: Scalars['FileHandle'];
  tags: ReadonlyArray<Scalars['String']> | Scalars['String'];
}>;

export type ISetTagsMutation = {
  readonly __typename?: 'Mutation';
  readonly setTags:
    | { readonly __typename: 'Directory'; readonly _id: string; readonly tags: ReadonlyArray<string> }
    | { readonly __typename: 'RegularFile'; readonly _id: string; readonly tags: ReadonlyArray<string> }
    | { readonly __typename: 'UnknownFile'; readonly _id: string; readonly tags: ReadonlyArray<string> };
};

export type ISetNotesMutationVariables = Exact<{
  fileHandle: Scalars['FileHandle'];
  notes: Scalars['String'];
}>;

export type ISetNotesMutation = {
  readonly __typename?: 'Mutation';
  readonly setNotes:
    | { readonly __typename: 'Directory'; readonly _id: string; readonly notes?: Maybe<string> }
    | { readonly __typename: 'RegularFile'; readonly _id: string; readonly notes?: Maybe<string> }
    | { readonly __typename: 'UnknownFile'; readonly _id: string; readonly notes?: Maybe<string> };
};

export const FileInfoFragmentDoc = gql`
  fragment FileInfo on File {
    _id
    __typename
    fileHandle {
      value
    }
    name
    mime
    icon
    thumbnail
    metadata
    tags
    notes
    stats {
      creationTime
      lastWriteTime
      size
    }
  }
`;
export const FileFragmentDoc = gql`
  fragment File on File {
    _id
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
export const SetTagsDocument = gql`
  mutation setTags($fileHandle: FileHandle!, $tags: [String!]!) {
    setTags(fileHandle: $fileHandle, tags: $tags) {
      _id
      __typename
      tags
    }
  }
`;
export type ISetTagsMutationFn = Apollo.MutationFunction<ISetTagsMutation, ISetTagsMutationVariables>;

/**
 * __useSetTagsMutation__
 *
 * To run a mutation, you first call `useSetTagsMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useSetTagsMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [setTagsMutation, { data, loading, error }] = useSetTagsMutation({
 *   variables: {
 *      fileHandle: // value for 'fileHandle'
 *      tags: // value for 'tags'
 *   },
 * });
 */
export function useSetTagsMutation(baseOptions?: Apollo.MutationHookOptions<ISetTagsMutation, ISetTagsMutationVariables>) {
  const options = { ...defaultOptions, ...baseOptions };
  return Apollo.useMutation<ISetTagsMutation, ISetTagsMutationVariables>(SetTagsDocument, options);
}
export type SetTagsMutationHookResult = ReturnType<typeof useSetTagsMutation>;
export type SetTagsMutationResult = Apollo.MutationResult<ISetTagsMutation>;
export type SetTagsMutationOptions = Apollo.BaseMutationOptions<ISetTagsMutation, ISetTagsMutationVariables>;
export const SetNotesDocument = gql`
  mutation setNotes($fileHandle: FileHandle!, $notes: String!) {
    setNotes(fileHandle: $fileHandle, notes: $notes) {
      _id
      __typename
      notes
    }
  }
`;
export type ISetNotesMutationFn = Apollo.MutationFunction<ISetNotesMutation, ISetNotesMutationVariables>;

/**
 * __useSetNotesMutation__
 *
 * To run a mutation, you first call `useSetNotesMutation` within a React component and pass it any options that fit your needs.
 * When your component renders, `useSetNotesMutation` returns a tuple that includes:
 * - A mutate function that you can call at any time to execute the mutation
 * - An object with fields that represent the current status of the mutation's execution
 *
 * @param baseOptions options that will be passed into the mutation, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options-2;
 *
 * @example
 * const [setNotesMutation, { data, loading, error }] = useSetNotesMutation({
 *   variables: {
 *      fileHandle: // value for 'fileHandle'
 *      notes: // value for 'notes'
 *   },
 * });
 */
export function useSetNotesMutation(baseOptions?: Apollo.MutationHookOptions<ISetNotesMutation, ISetNotesMutationVariables>) {
  const options = { ...defaultOptions, ...baseOptions };
  return Apollo.useMutation<ISetNotesMutation, ISetNotesMutationVariables>(SetNotesDocument, options);
}
export type SetNotesMutationHookResult = ReturnType<typeof useSetNotesMutation>;
export type SetNotesMutationResult = Apollo.MutationResult<ISetNotesMutation>;
export type SetNotesMutationOptions = Apollo.BaseMutationOptions<ISetNotesMutation, ISetNotesMutationVariables>;

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
