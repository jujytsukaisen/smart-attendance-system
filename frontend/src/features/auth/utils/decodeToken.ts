import type { User } from '../types';

interface JwtClaims {
    sub: string;
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name': string;
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress': string;
    'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': string | string[];
    Permission?: string | string[];
    CompanyId?: string;
}

export const decodeUserFromToken = (token: string): User => {
    const payloadBase64 = token.split('.')[1];
    const payloadJson = decodeURIComponent(
        atob(payloadBase64.replace(/-/g, '+').replace(/_/g, '/'))
            .split('')
            .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
            .join('')
    );
    const claims: JwtClaims = JSON.parse(payloadJson);

    const roles = claims['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    const permissions = claims.Permission;

    return {
        id: claims.sub,
        email: claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'],
        firstName: claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || '',
        lastName: '',
        roles: Array.isArray(roles) ? roles : roles ? [roles] : [],
        permissions: Array.isArray(permissions) ? permissions : permissions ? [permissions] : [],
    };
};