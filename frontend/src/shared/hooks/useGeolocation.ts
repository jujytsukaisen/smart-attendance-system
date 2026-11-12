import { useEffect, useState, useRef, useCallback } from 'react';

export interface LocationCoords {
    latitude: number;
    longitude: number;
    accuracy?: number;
    timestamp?: number;
    source?: 'gps' | 'ip' | 'manual';
}

export type GpsStatus = 'loading' | 'granted' | 'denied' | 'unavailable' | 'insecure' | 'error';

async function fetchIpLocation(): Promise<LocationCoords | null> {
    try {
        const response = await fetch('https://ipwho.is/', { signal: AbortSignal.timeout(4000) });
        if (response.ok) {
            const data = await response.json();
            if (data.success && typeof data.latitude === 'number' && typeof data.longitude === 'number') {
                return {
                    latitude: data.latitude,
                    longitude: data.longitude,
                    accuracy: 1000,
                    timestamp: Date.now(),
                    source: 'ip',
                };
            }
        }
    } catch {
        // Ignore IP fallback errors
    }
    return null;
}

export function useGeolocation(enableHighAccuracy = true) {
    const [coords, setCoords] = useState<LocationCoords | null>(null);
    const [status, setStatus] = useState<GpsStatus>('loading');
    const [errorMessage, setErrorMessage] = useState<string | null>(null);

    const coordsRef = useRef<LocationCoords | null>(null);
    coordsRef.current = coords;

    const handleSuccess = useCallback((pos: GeolocationPosition) => {
        const newCoords: LocationCoords = {
            latitude: pos.coords.latitude,
            longitude: pos.coords.longitude,
            accuracy: pos.coords.accuracy,
            timestamp: pos.timestamp,
            source: 'gps',
        };
        coordsRef.current = newCoords;
        setCoords(newCoords);
        setStatus('granted');
        setErrorMessage(null);
    }, []);

    const handleError = useCallback(async (err: GeolocationPositionError, isRetryFallback = false) => {
        // 1. If high accuracy failed on desktop, try low accuracy
        if (!isRetryFallback && typeof window !== 'undefined' && navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(
                handleSuccess,
                (fallbackErr) => handleError(fallbackErr, true),
                {
                    enableHighAccuracy: false,
                    timeout: 8000,
                    maximumAge: 10000,
                }
            );
            return;
        }

        // 2. Try IP-based location fallback if browser GPS hardware is disabled or unavailable
        const ipLocation = await fetchIpLocation();
        if (ipLocation) {
            coordsRef.current = ipLocation;
            setCoords(ipLocation);
            setStatus('granted');
            setErrorMessage(null);
            return;
        }

        let msg = 'Failed to retrieve location.';
        switch (err.code) {
            case err.PERMISSION_DENIED:
                setStatus('denied');
                msg = 'GPS permission denied. Please allow location in browser settings.';
                break;
            case err.POSITION_UNAVAILABLE:
                setStatus('unavailable');
                msg = 'Windows Location is OFF. Please turn on Location in Windows Settings > Privacy & security > Location.';
                break;
            case err.TIMEOUT:
                setStatus('error');
                msg = 'Location request timed out. Please click retry.';
                break;
            default:
                setStatus('error');
                break;
        }
        setErrorMessage(msg);
    }, [handleSuccess]);

    const requestLocation = useCallback(async () => {
        if (typeof window === 'undefined') return;

        // Check for secure context (HTTPS or localhost)
        const isSecure = window.isSecureContext || 
            window.location.hostname === 'localhost' || 
            window.location.hostname === '127.0.0.1';

        if (!isSecure) {
            // Attempt IP location for insecure context (e.g. mobile over http://192.168.x.x)
            const ipLocation = await fetchIpLocation();
            if (ipLocation) {
                coordsRef.current = ipLocation;
                setCoords(ipLocation);
                setStatus('granted');
                setErrorMessage(null);
                return;
            }
            setStatus('insecure');
            setErrorMessage('GPS requires HTTPS or localhost on modern browsers.');
            return;
        }

        if (!navigator.geolocation) {
            const ipLocation = await fetchIpLocation();
            if (ipLocation) {
                coordsRef.current = ipLocation;
                setCoords(ipLocation);
                setStatus('granted');
                setErrorMessage(null);
                return;
            }
            setStatus('unavailable');
            setErrorMessage('Geolocation is not supported by this browser.');
            return;
        }

        setStatus('loading');

        navigator.geolocation.getCurrentPosition(
            handleSuccess,
            (err) => handleError(err, false),
            {
                enableHighAccuracy,
                timeout: 8000,
                maximumAge: 0,
            }
        );
    }, [enableHighAccuracy, handleSuccess, handleError]);

    useEffect(() => {
        requestLocation();

        if (typeof window === 'undefined' || !navigator.geolocation) return;

        const isSecure = window.isSecureContext || 
            window.location.hostname === 'localhost' || 
            window.location.hostname === '127.0.0.1';

        if (!isSecure) return;

        // Watch continuous position
        const watchId = navigator.geolocation.watchPosition(
            handleSuccess,
            (err) => {
                if (!coordsRef.current) {
                    handleError(err, false);
                }
            },
            {
                enableHighAccuracy,
                timeout: 15000,
                maximumAge: 5000,
            }
        );

        return () => {
            navigator.geolocation.clearWatch(watchId);
        };
    }, [requestLocation, enableHighAccuracy, handleSuccess, handleError]);

    const getCurrentCoords = useCallback(async (): Promise<LocationCoords> => {
        if (coordsRef.current && (Date.now() - (coordsRef.current.timestamp ?? 0) < 60000)) {
            return coordsRef.current;
        }

        return new Promise((resolve) => {
            if (typeof window === 'undefined' || !navigator.geolocation) {
                fetchIpLocation().then((ipLoc) => {
                    resolve(ipLoc ?? { latitude: 0, longitude: 0 });
                });
                return;
            }

            navigator.geolocation.getCurrentPosition(
                (pos) => {
                    const fresh: LocationCoords = {
                        latitude: pos.coords.latitude,
                        longitude: pos.coords.longitude,
                        accuracy: pos.coords.accuracy,
                        timestamp: pos.timestamp,
                        source: 'gps',
                    };
                    coordsRef.current = fresh;
                    setCoords(fresh);
                    setStatus('granted');
                    resolve(fresh);
                },
                () => {
                    // Try fallback without high accuracy
                    navigator.geolocation.getCurrentPosition(
                        (pos) => {
                            const fresh: LocationCoords = {
                                latitude: pos.coords.latitude,
                                longitude: pos.coords.longitude,
                                accuracy: pos.coords.accuracy,
                                timestamp: pos.timestamp,
                                source: 'gps',
                            };
                            coordsRef.current = fresh;
                            setCoords(fresh);
                            setStatus('granted');
                            resolve(fresh);
                        },
                        async () => {
                            const ipLoc = await fetchIpLocation();
                            if (ipLoc) {
                                coordsRef.current = ipLoc;
                                setCoords(ipLoc);
                                setStatus('granted');
                                resolve(ipLoc);
                            } else {
                                resolve(coordsRef.current ?? { latitude: 0, longitude: 0 });
                            }
                        },
                        {
                            enableHighAccuracy: false,
                            timeout: 6000,
                            maximumAge: 10000,
                        }
                    );
                },
                {
                    enableHighAccuracy: true,
                    timeout: 6000,
                    maximumAge: 5000,
                }
            );
        });
    }, []);

    const setManualCoords = useCallback((latitude: number, longitude: number) => {
        const manual: LocationCoords = {
            latitude,
            longitude,
            accuracy: 10,
            timestamp: Date.now(),
            source: 'manual',
        };
        coordsRef.current = manual;
        setCoords(manual);
        setStatus('granted');
        setErrorMessage(null);
    }, []);

    return {
        coords,
        status,
        errorMessage,
        getCurrentCoords,
        setManualCoords,
        retry: requestLocation,
    };
}
