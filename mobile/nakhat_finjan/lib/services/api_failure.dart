import 'package:dio/dio.dart';

import '../models/api_error.dart';

/// Turns a transport/HTTP failure into the API's `{ code, message }` shape.
///
/// Every non-2xx from this backend carries that body (a middleware guarantees
/// it), so the fallbacks below only fire when the request never reached the API
/// or something upstream — a proxy, a tunnel — answered instead.
///
/// Every service maps its failures through this one function, so a caller can
/// branch on [ApiException.code] without caring which endpoint it came from,
/// and no service leaks a `DioException` upwards.
ApiException toApiException(DioException e) {
  final data = e.response?.data;
  final status = e.response?.statusCode;

  if (data is Map<String, dynamic> && data['code'] is String) {
    return ApiException(ApiError.fromJson(data), statusCode: status);
  }

  if (e.response == null) {
    return ApiException(
      const ApiError(
        code: ErrorCodes.networkError,
        message: 'تعذّر الاتصال بالخادم، تحقّق من الإنترنت وحاول مجدداً',
      ),
      statusCode: status,
    );
  }

  return ApiException(
    const ApiError(
      code: ErrorCodes.internalError,
      message: 'حدث خطأ غير متوقع، حاول مجدداً',
    ),
    statusCode: status,
  );
}

/// The failure raised when a 2xx arrives with nothing in it.
///
/// A body-less success is a server bug rather than a transport problem, so it
/// gets the generic internal code rather than the network one — retrying would
/// not help, and telling the customer to check their connection would be a lie.
ApiException emptyBodyFailure() => const ApiException(
  ApiError(
    code: ErrorCodes.internalError,
    message: 'تعذّر قراءة استجابة الخادم',
  ),
);
