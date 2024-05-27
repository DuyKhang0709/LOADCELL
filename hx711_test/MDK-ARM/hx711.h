#ifndef delay_h
#define delay_h

#include "stm32f4xx_hal.h"
#include "Delay.h"

extern int32_t getHX711(void);
extern float get_weight_avg(uint16_t samples);
extern int get_zero();
extern float get_span(float know_weight_max,float zero2);
extern float get_weight_calib(float zero_calib,float span_calib);
extern void hx711_init(void);

#define DT_PIN GPIO_PIN_11
#define DT_PORT GPIOE

#define SCK_PIN GPIO_PIN_5
#define SCK_PORT GPIOC

#endif