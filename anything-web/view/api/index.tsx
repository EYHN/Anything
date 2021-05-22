import { gql } from '@apollo/client';
import * as Apollo from '@apollo/client';
export type Maybe<T> = T | null;
export type Exact<T extends { [key: string]: unknown }> = { [K in keyof T]: T[K] };
export type MakeOptional<T, K extends keyof T> = Omit<T, K> & { [SubKey in K]?: Maybe<T[SubKey]> };
export type MakeMaybe<T, K extends keyof T> = Omit<T, K> & { [SubKey in K]: Maybe<T[SubKey]> };
/** All built-in and custom scalars, mapped to their actual values */
export type Scalars = {
  ID: string;
  String: string;
  Boolean: boolean;
  Int: number;
  Float: number;
  /** The `Date` scalar type represents a year, month and day in accordance with the [ISO-8601](https://en.wikipedia.org/wiki/ISO_8601) standard. */
  Date: any;
  /** The `DateTime` scalar type represents a date and time. `DateTime` expects timestamps to be formatted in accordance with the [ISO-8601](https://en.wikipedia.org/wiki/ISO_8601) standard. */
  DateTime: any;
  /** The `DateTimeOffset` scalar type represents a date, time and offset from UTC. `DateTimeOffset` expects timestamps to be formatted in accordance with the [ISO-8601](https://en.wikipedia.org/wiki/ISO_8601) standard. */
  DateTimeOffset: any;
  /** The `Seconds` scalar type represents a period of time represented as the total number of seconds. */
  Seconds: any;
  /** The `Milliseconds` scalar type represents a period of time represented as the total number of milliseconds. */
  Milliseconds: any;
  Decimal: any;
  Uri: any;
  Guid: any;
  Short: any;
  UShort: any;
  UInt: any;
  Long: any;
  BigInt: any;
  ULong: any;
  Byte: any;
  SByte: any;
  Json: any;
};

















/** The query type, represents all of the entry points into our object graph. */
export type IQuery = {
  readonly __typename?: 'Query';
  /** Query a directory */
  readonly directory: IDirectory;
  /** Query a file */
  readonly file: IFile;
};


/** The query type, represents all of the entry points into our object graph. */
export type IQueryDirectoryArgs = {
  path: Scalars['String'];
};


/** The query type, represents all of the entry points into our object graph. */
export type IQueryFileArgs = {
  path: Scalars['String'];
};

/** A directory is a location for storing files. */
export type IDirectory = IFile & {
  readonly __typename?: 'Directory';
  /** Dynamic icon path of the directory. */
  readonly dynamicIcon?: Maybe<Scalars['String']>;
  /** Entries of the directory. */
  readonly entries: ReadonlyArray<IFile>;
  /** Icon path of the directory. */
  readonly icon: Scalars['String'];
  /** Media type about the directory. */
  readonly mime?: Maybe<Scalars['String']>;
  /** Name of the directory. */
  readonly name: Scalars['String'];
  /** Represents the fully qualified path of this directory. */
  readonly path: Scalars['String'];
  /** Information about the directory. */
  readonly stats: IFileStats;
};

/** A FileStats object provides information about a file. */
export type IFileStats = {
  readonly __typename?: 'FileStats';
  /** The last time this file was accessed. */
  readonly accessTime?: Maybe<Scalars['DateTimeOffset']>;
  /** The creation time of the file. */
  readonly creationTime?: Maybe<Scalars['DateTimeOffset']>;
  /** The last time this file was modified. */
  readonly modifyTime?: Maybe<Scalars['DateTimeOffset']>;
  /** The size of the file in bytes. */
  readonly size?: Maybe<Scalars['Long']>;
};

/** A File object can represent either a file or a directory. */
export type IFile = {
  /** Dynamic icon path of the file. */
  readonly dynamicIcon?: Maybe<Scalars['String']>;
  /** Icon path of the file. */
  readonly icon: Scalars['String'];
  /** Media type about the file. */
  readonly mime?: Maybe<Scalars['String']>;
  /** Name of the file. */
  readonly name: Scalars['String'];
  /** Represents the fully qualified path of the directory or file. */
  readonly path: Scalars['String'];
  /** Information about the file. */
  readonly stats: IFileStats;
};

/** The mutation type, represents all updates we can make to our data. */
export type IMutation = {
  readonly __typename?: 'Mutation';
  readonly hello?: Maybe<Scalars['String']>;
};


/** The mutation type, represents all updates we can make to our data. */
export type IMutationHelloArgs = {
  world?: Maybe<Scalars['String']>;
};

/** A regular file is a file that is not a directory and is not some special kind of file such as a device. */
export type IRegularFile = IFile & {
  readonly __typename?: 'RegularFile';
  /** Dynamic icon path of the file. */
  readonly dynamicIcon?: Maybe<Scalars['String']>;
  /** Icon path of the file. */
  readonly icon: Scalars['String'];
  /** Metadata of the file. */
  readonly metadata?: Maybe<Scalars['Json']>;
  /** Media type about the file. */
  readonly mime?: Maybe<Scalars['String']>;
  /** The name of the file. */
  readonly name: Scalars['String'];
  /** Represents the fully qualified path of this file. */
  readonly path: Scalars['String'];
  /** Information about the file. */
  readonly stats: IFileStats;
};


type IListDirectoryEntry_Directory_Fragment = (
  { readonly __typename: 'Directory' }
  & Pick<IDirectory, 'name' | 'path' | 'mime' | 'icon' | 'dynamicIcon'>
);

type IListDirectoryEntry_RegularFile_Fragment = (
  { readonly __typename: 'RegularFile' }
  & Pick<IRegularFile, 'name' | 'path' | 'mime' | 'icon' | 'dynamicIcon'>
);

export type IListDirectoryEntryFragment = IListDirectoryEntry_Directory_Fragment | IListDirectoryEntry_RegularFile_Fragment;

export type IListDirectoryFragment = (
  { readonly __typename?: 'Directory' }
  & { readonly entries: ReadonlyArray<(
    { readonly __typename?: 'Directory' }
    & IListDirectoryEntry_Directory_Fragment
  ) | (
    { readonly __typename?: 'RegularFile' }
    & IListDirectoryEntry_RegularFile_Fragment
  )> }
);

export type IListDirectoryQueryVariables = Exact<{
  path: Scalars['String'];
}>;


export type IListDirectoryQuery = (
  { readonly __typename?: 'Query' }
  & { readonly directory: (
    { readonly __typename?: 'Directory' }
    & IListDirectoryFragment
  ) }
);

type IOpenFile_Directory_Fragment = (
  { readonly __typename: 'Directory' }
  & Pick<IDirectory, 'path' | 'name' | 'icon' | 'mime' | 'dynamicIcon'>
  & { readonly stats: (
    { readonly __typename?: 'FileStats' }
    & Pick<IFileStats, 'accessTime' | 'creationTime' | 'modifyTime' | 'size'>
  ) }
);

type IOpenFile_RegularFile_Fragment = (
  { readonly __typename: 'RegularFile' }
  & Pick<IRegularFile, 'metadata' | 'path' | 'name' | 'icon' | 'mime' | 'dynamicIcon'>
  & { readonly stats: (
    { readonly __typename?: 'FileStats' }
    & Pick<IFileStats, 'accessTime' | 'creationTime' | 'modifyTime' | 'size'>
  ) }
);

export type IOpenFileFragment = IOpenFile_Directory_Fragment | IOpenFile_RegularFile_Fragment;

export type IOpenQueryVariables = Exact<{
  path: Scalars['String'];
}>;


export type IOpenQuery = (
  { readonly __typename?: 'Query' }
  & { readonly file: (
    { readonly __typename?: 'Directory' }
    & IOpenFile_Directory_Fragment
  ) | (
    { readonly __typename?: 'RegularFile' }
    & IOpenFile_RegularFile_Fragment
  ) }
);

export const ListDirectoryEntryFragmentDoc = gql`
    fragment ListDirectoryEntry on File {
  __typename
  name
  path
  mime
  icon
  dynamicIcon
}
    `;
export const ListDirectoryFragmentDoc = gql`
    fragment ListDirectory on Directory {
  entries {
    ...ListDirectoryEntry
  }
}
    ${ListDirectoryEntryFragmentDoc}`;
export const OpenFileFragmentDoc = gql`
    fragment OpenFile on File {
  __typename
  path
  name
  icon
  mime
  dynamicIcon
  stats {
    accessTime
    creationTime
    modifyTime
    size
  }
  ... on RegularFile {
    metadata
  }
}
    `;
export const ListDirectoryDocument = gql`
    query ListDirectory($path: String!) {
  directory(path: $path) {
    ...ListDirectory
  }
}
    ${ListDirectoryFragmentDoc}`;

/**
 * __useListDirectoryQuery__
 *
 * To run a query within a React component, call `useListDirectoryQuery` and pass it any options that fit your needs.
 * When your component renders, `useListDirectoryQuery` returns an object from Apollo Client that contains loading, error, and data properties
 * you can use to render your UI.
 *
 * @param baseOptions options that will be passed into the query, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options;
 *
 * @example
 * const { data, loading, error } = useListDirectoryQuery({
 *   variables: {
 *      path: // value for 'path'
 *   },
 * });
 */
export function useListDirectoryQuery(baseOptions: Apollo.QueryHookOptions<IListDirectoryQuery, IListDirectoryQueryVariables>) {
        return Apollo.useQuery<IListDirectoryQuery, IListDirectoryQueryVariables>(ListDirectoryDocument, baseOptions);
      }
export function useListDirectoryLazyQuery(baseOptions?: Apollo.LazyQueryHookOptions<IListDirectoryQuery, IListDirectoryQueryVariables>) {
          return Apollo.useLazyQuery<IListDirectoryQuery, IListDirectoryQueryVariables>(ListDirectoryDocument, baseOptions);
        }
export type ListDirectoryQueryHookResult = ReturnType<typeof useListDirectoryQuery>;
export type ListDirectoryLazyQueryHookResult = ReturnType<typeof useListDirectoryLazyQuery>;
export type ListDirectoryQueryResult = Apollo.QueryResult<IListDirectoryQuery, IListDirectoryQueryVariables>;
export const OpenDocument = gql`
    query Open($path: String!) {
  file(path: $path) {
    ...OpenFile
  }
}
    ${OpenFileFragmentDoc}`;

/**
 * __useOpenQuery__
 *
 * To run a query within a React component, call `useOpenQuery` and pass it any options that fit your needs.
 * When your component renders, `useOpenQuery` returns an object from Apollo Client that contains loading, error, and data properties
 * you can use to render your UI.
 *
 * @param baseOptions options that will be passed into the query, supported options are listed on: https://www.apollographql.com/docs/react/api/react-hooks/#options;
 *
 * @example
 * const { data, loading, error } = useOpenQuery({
 *   variables: {
 *      path: // value for 'path'
 *   },
 * });
 */
export function useOpenQuery(baseOptions: Apollo.QueryHookOptions<IOpenQuery, IOpenQueryVariables>) {
        return Apollo.useQuery<IOpenQuery, IOpenQueryVariables>(OpenDocument, baseOptions);
      }
export function useOpenLazyQuery(baseOptions?: Apollo.LazyQueryHookOptions<IOpenQuery, IOpenQueryVariables>) {
          return Apollo.useLazyQuery<IOpenQuery, IOpenQueryVariables>(OpenDocument, baseOptions);
        }
export type OpenQueryHookResult = ReturnType<typeof useOpenQuery>;
export type OpenLazyQueryHookResult = ReturnType<typeof useOpenLazyQuery>;
export type OpenQueryResult = Apollo.QueryResult<IOpenQuery, IOpenQueryVariables>;

      export interface PossibleTypesResultData {
        possibleTypes: {
          [key: string]: string[]
        }
      }
      const result: PossibleTypesResultData = {
  "possibleTypes": {
    "File": [
      "Directory",
      "RegularFile"
    ]
  }
};
      export default result;
    